package com.company;


import java.io.*;
import java.math.BigInteger;
import java.net.Socket;
import java.security.MessageDigest;
import java.security.NoSuchAlgorithmException;
import java.util.Random;
import java.util.Scanner;

public class Main {

    // https://dustri.org/b/social-millionaires-protocol-in-otr.html
    private static BigInteger p;
    private static BigInteger g;
    private static PrintWriter out;
    private static BufferedReader in;
    private static Random rnd;
    private static BigInteger sharedSecret;
    private static BigInteger dhSharedKey;

    public static void main(String[] args) {
        String serverName = "eitn41.eit.lth.se";
        int port = 1337;
        rnd = new Random();
        String pString = "FFFFFFFFFFFFFFFFC90FDAA22168C234C4C6628B80DC1CD129024E088A67CC74020BBEA63B139B22514A08798E3404DDEF9519B3CD3A431B302B0A6DF25F14374FE1356D6D51C245E485B576625E7EC6F44C42E9A637ED6B0BFF5CB6F406B7EDEE386BFB5A899FA5AE9F24117C4B1FE649286651ECE45B3DC2007CB8A163BF0598DA48361C55D39A69163FA8FD24CF5F83655D23DCA3AD961C62F356208552BB9ED529077096966D670C354E4ABC9804F1746C08CA237327FFFFFFFFFFFFFFFF";
        p = new BigInteger(pString, 16);
        g = new BigInteger("2");

        try {
            Socket client = new Socket(serverName, port);
            out = new PrintWriter(client.getOutputStream(), true);
            in = new BufferedReader(new InputStreamReader(client.getInputStream()));

            BigInteger x2 = generateNumber();
            dhSharedKey = diffieHellman(x2);
            String passphrase = "eitn41 <3";

            MessageDigest mDigest = MessageDigest.getInstance("SHA1");
            byte[] result = mDigest.digest(conc(dhSharedKey.toByteArray(), passphrase.getBytes()));
            sharedSecret = new BigInteger(result);

            boolean cont = SMB();

            if(cont)
                secureChat();
            client.close();
        } catch (IOException e) {
            e.printStackTrace();
        } catch (NoSuchAlgorithmException e) {
            e.printStackTrace();
        }
    }

    private static BigInteger diffieHellman (BigInteger nbr) throws IOException {
        BigInteger g_x = getNumber("g^x");

        BigInteger g_y = g.modPow(nbr, p); // g^b mod p
        sendNumber(g_y, "g^x2");

        return g_x.modPow(nbr, p);
    }

    private static boolean SMB() throws IOException {
        BigInteger b2 = generateNumber();
        BigInteger g2 = diffieHellman(b2);

        BigInteger b3 = generateNumber();
        BigInteger g3 = diffieHellman(b3);

        BigInteger b = generateNumber();

        BigInteger pa = getNumber("P_a");
        BigInteger pb = g3.modPow(b, p); // P_b = g_3^r
        sendNumber(pb, "P_b");

        BigInteger qa = getNumber("Q_a");
        BigInteger qb = calculateQb(b, g2); // Q_b
        sendNumber(qb, "Q_b");

        BigInteger ra = getNumber("R_a");
        BigInteger rb = calculateRb(qa, qb, b3); // R_b = (Q_a / Q_b)^b_3
        sendNumber(rb, "R_b");

        System.out.println("Calculation check result: " + calculateCheck(ra, b3, pa, pb));
        String authenticatedSucceded =  in.readLine();
        System.out.println("Authentication: " + authenticatedSucceded);

        return authenticatedSucceded.equals("ack");
    }

    private static BigInteger calculateQb(BigInteger b, BigInteger g2){ // Q_b = g_1^r*g_2^x2
        BigInteger gr = g.modPow(b, p);
        BigInteger gb2y = g2.modPow(sharedSecret, p);

        BigInteger qb = modMultipy(gr, gb2y);
        return qb;
    }

    private static BigInteger calculateRb(BigInteger qa, BigInteger qb, BigInteger b3){ // R_b = (Q_a / Q_b)^b_3
        BigInteger inv = invm(qb);
        BigInteger mul = modMultipy(qa, inv);
        BigInteger r_b = mul.modPow(b3, p);
        return r_b;
    }

    private static boolean calculateCheck(BigInteger ra, BigInteger b3, BigInteger pa, BigInteger pb){
        BigInteger r_ab = ra.modPow(b3, p);
        BigInteger inv = invm(pb);
        BigInteger mulm = modMultipy(pa, inv);

        return r_ab.equals(mulm);
    }

    private static void sendNumber(BigInteger nbr, String name) throws IOException {
        System.out.println("Sending " + name + ": " + nbr.toString(16));
        out.println(nbr.toString(16));
        System.out.println("Acknowledgement " + name + ": " + in.readLine());
    }

    private static BigInteger getNumber(String name) throws IOException {
        String nbr = in.readLine();
        System.out.println("Recived " + name + ": " + nbr);
        return new BigInteger(nbr, 16);
    }

    private static BigInteger generateNumber() {
        BigInteger generated = new BigInteger(1536, rnd);
        BigInteger inMod = generated.mod(p);
        return inMod;
    }

    private static BigInteger invm(BigInteger x){
        return x.modPow(p.subtract(new BigInteger("2")), p);
    }

    private static BigInteger modMultipy(BigInteger x, BigInteger y){
        BigInteger mul = x.multiply(y);
        return mul.mod(p);
    }

    private static byte[] conc(byte[]a, byte[]b){
        byte[] c = new byte[a.length + b.length];
        System.arraycopy(a, 0, c, 0, a.length);
        System.arraycopy(b, 0, c, a.length, b.length);
        return c;
    }


    private static void secureChat() throws IOException {
        BigInteger msg = getMsgToSend();

        BigInteger enc_msg = msg.xor(dhSharedKey);
        String enc_msg_str = enc_msg.toString(16);


        System.out.println("Sending msg: " + enc_msg_str);
        out.println(enc_msg_str);
        String response = in.readLine();
        System.out.println("Response: " + response);
    }

    private static BigInteger getMsgToSend() {
        Scanner consoleIn = new Scanner(System.in);
        System.out.print("What is the message to send?: ");
        String msg_str = consoleIn.nextLine();
        return new BigInteger(msg_str, 16);
    }
}
