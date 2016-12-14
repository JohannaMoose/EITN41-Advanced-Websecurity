import hashlib
import os
import random
import struct

class SMP(object):
    def __init__(self, secret=None):
        self.p = 2410312426921032588552076022197566074856950548502459942654116941958108831682612228890093858261341614673227141477904012196503648957050582631942730706805009223062734745341073406696246014589361659774041027169249453200378729434170325843778659198143763193776859869524088940195577346119843545301547043747207749969763750084308926339295559968882457872412993810129130294592999947926365264059284647209730384947211681434464714438488520940127459844288859336526896320919633919
        self.modOrder = (self.p - 1) / 2
        self.g = 2
        self.match = False

        if type(secret) is str:
            # Encode the string as a hex value
            self.secret = int(secret.encode('hex'), 16)
        elif type(secret) is int or type(secret) is long:
            self.secret = secret
        else:
            raise TypeError("Secret must be an int or a string. Got type: " + str(type(secret)))

    def step1(self):
        self.b2 = createRandomExponent()
        self.b3 = createRandomExponent()

        self.g2 = pow(self.g, self.b2, self.p)
        self.g3 = pow(self.g, self.b3, self.p)

        (c1, d1) = self.createLogProof('1', self.b2)
        (c2, d2) = self.createLogProof('2', self.b3)

        # Send g2a, g3a, c1, d1, c2, d2
        return packList(self.g2, self.g3, c1, d1, c2, d2)

    def step1ForB(self, buffer):
        (g2a, g3a, c1, d1, c2, d2) = unpackList(buffer)

        if not self.isValidArgument(g2a) or not self.isValidArgument(g3a):
            raise ValueError("Invalid g2a/g3a values")

        if not self.checkLogProof('1', g2a, c1, d1):
            raise ValueError("Proof 1 check failed")

        if not self.checkLogProof('2', g3a, c2, d2):
            raise ValueError("Proof 2 check failed")

        self.g2a = g2a
        self.g3a = g3a

        self.b2 = createRandomExponent()
        self.b3 = createRandomExponent()

        b = createRandomExponent()

        self.g2 = pow(self.g, self.b2, self.p)
        self.g3 = pow(self.g, self.b3, self.p)

        (c3, d3) = self.createLogProof('3', self.b2)
        (c4, d4) = self.createLogProof('4', self.b3)

        self.gb2 = pow(self.g2a, self.b2, self.p)
        self.gb3 = pow(self.g3a, self.b3, self.p)

        self.pb = pow(self.gb3, b, self.p)
        self.qb = mulm(pow(self.g, b, self.p), pow(self.gb2, self.secret, self.p), self.p)

        (c5, d5, d6) = self.createCoordsProof('5', self.gb2, self.gb3, b)

        # Sends g2b, g3b, pb, qb, all the c's and d's
        return packList(self.g2, self.g3, self.pb, self.qb, c3, d3, c4, d4, c5, d5, d6)

    def step3(self, buffer):
        (g2b, g3b, pb, qb, c3, d3, c4, d4, c5, d5, d6) = unpackList(buffer)

        if not self.isValidArgument(g2b) or not self.isValidArgument(g3b) or \
           not self.isValidArgument(pb) or not self.isValidArgument(qb):
            raise ValueError("Invalid g2b/g3b/pb/qb values")

        if not self.checkLogProof('3', g2b, c3, d3):
            raise ValueError("Proof 3 check failed")

        if not self.checkLogProof('4', g3b, c4, d4):
            raise ValueError("Proof 4 check failed")

        self.g2b = g2b
        self.g3b = g3b

        self.ga2 = pow(self.g2b, self.b2, self.p)
        self.ga3 = pow(self.g3b, self.b3, self.p)

        if not self.checkCoordsProof('5', c5, d5, d6, self.ga2, self.ga3, pb, qb):
            raise ValueError("Proof 5 check failed")

        s = createRandomExponent()

        self.qb = qb
        self.pb = pb
        self.pa = pow(self.ga3, s, self.p)
        self.qa = mulm(pow(self.g, s, self.p), pow(self.ga2, self.secret, self.p), self.p)

        (c6, d7, d8) = self.createCoordsProof('6', self.ga2, self.ga3, s)

        inv = self.invm(qb)
        self.ra = pow(mulm(self.qa, inv, self.p), self.b3, self.p)

        (c7, d9) = self.createEqualLogsProof('7', self.qa, inv, self.b3)

        # Sends pa, qa, ra, c6, d7, d8, c7, d9
        return packList(self.pa, self.qa, self.ra, c6, d7, d8, c7, d9)

    def step4(self, buffer):
        (pa, qa, ra, c6, d7, d8, c7, d9) = unpackList(buffer)

        if not self.isValidArgument(pa) or not self.isValidArgument(qa) or not self.isValidArgument(ra):
            raise ValueError("Invalid pa/qa/ra values")

        if not self.checkCoordsProof('6', c6, d7, d8, self.gb2, self.gb3, pa, qa):
            raise ValueError("Proof 6 check failed")

        if not self.checkEqualLogs('7', c7, d9, self.g3a, mulm(qa, self.invm(self.qb), self.p), ra):
            raise ValueError("Proof 7 check failed")

        inv = self.invm(self.qb)
        rb = pow(mulm(qa, inv, self.p), self.b3, self.p)

        (c8, d10) = self.createEqualLogsProof('8', qa, inv, self.b3)

        rab = pow(ra, self.b3, self.p)


        inv = self.invm(self.pb)
        if rab == mulm(pa, inv, self.p):
            self.match = True

        # Send rb, c8, d10
        return packList(rb, c8, d10)

    def step5(self, buffer):
        (rb, c8, d10) = unpackList(buffer)

        if not self.isValidArgument(rb):
            raise ValueError("Invalid rb values")

        if not self.checkEqualLogs('8', c8, d10, self.g3b, mulm(self.qa, self.invm(self.qb), self.p), rb):
            raise ValueError("Proof 8 check failed")

        rab = pow(rb, self.b3, self.p)

        inv = self.invm(self.pb)
        if rab == mulm(self.pa, inv, self.p):
            self.match = True

    def createLogProof(self, version, x):
        randExponent = createRandomExponent()
        c = sha256(version + str(pow(self.g, randExponent, self.p)))
        d = (randExponent - mulm(x, c, self.modOrder)) % self.modOrder
        return (c, d)

    def checkLogProof(self, version, g, c, d):
        gd = pow(self.g, d, self.p)
        gc = pow(g, c, self.p)
        gdgc = gd * gc % self.p
        return (sha256(version + str(gdgc)) == c)

    def createCoordsProof(self, version, g2, g3, r):
        r1 = createRandomExponent()
        r2 = createRandomExponent()

        tmp1 = pow(g3, r1, self.p)
        tmp2 = mulm(pow(self.g, r1, self.p), pow(g2, r2, self.p), self.p)

        c = sha256(version + str(tmp1) + str(tmp2))

        # TODO: make a subm function
        d1 = (r1 - mulm(r, c, self.modOrder)) % self.modOrder
        d2 = (r2 - mulm(self.secret, c, self.modOrder)) % self.modOrder

        return (c, d1, d2)

    def checkCoordsProof(self, version, c, d1, d2, g2, g3, p, q):
        tmp1 = mulm(pow(g3, d1, self.p), pow(p, c, self.p), self.p)

        tmp2 = mulm(mulm(pow(self.g, d1, self.p), pow(g2, d2, self.p), self.p), pow(q, c, self.p), self.p)

        cprime = sha256(version + str(tmp1) + str(tmp2))

        return (c == cprime)

    def createEqualLogsProof(self, version, qa, qb, x):
        r = createRandomExponent()
        tmp1 = pow(self.g, r, self.p)
        qab = mulm(qa, qb, self.p)
        tmp2 = pow(qab, r, self.p)

        c = sha256(version + str(tmp1) + str(tmp2))
        tmp1 = mulm(x, c, self.modOrder)
        d = (r - tmp1) % self.modOrder

        return (c, d)

    def checkEqualLogs(self, version, c, d, g3, qab, r):
        tmp1 = mulm(pow(self.g, d, self.p), pow(g3, c, self.p), self.p)

        tmp2 = mulm(pow(qab, d, self.p), pow(r, c, self.p), self.p)

        cprime = sha256(version + str(tmp1) + str(tmp2))
        return (c == cprime)

    def invm(self, x):
        return pow(x, self.p - 2, self.p)

    def isValidArgument(self, val):
        return (val >= 2 and val <= self.p - 2)

def packList(*items):
    buffer = ''

    # For each item in the list, convert it to a byte string and add its length as a prefix
    for item in items:
        bytes = longToBytes(item)
        buffer += struct.pack('!I', len(bytes)) + bytes

    return buffer

def unpackList(buffer):
    items = []

    index = 0
    while index < len(buffer):
        # Get the length of the long (4 byte int before the actual long)
        length = struct.unpack('!I', buffer[index:index+4])[0]
        index += 4

        # Convert the data back to a long and add it to the list
        item = bytesToLong(buffer[index:index+length])
        items.append(item)
        index += length

    return items

def bytesToLong(bytes):
    length = len(bytes)
    string = 0
    for i in range(length):
        string += byteToLong(bytes[i:i+1]) << 8*(length-i-1)
    return string

def longToBytes(long):
    bytes = ''
    while long != 0:
        bytes = longToByte(long & 0xff) + bytes
        long >>= 8
    return bytes

def byteToLong(byte):
    return struct.unpack('B', byte)[0]

def longToByte(long):
    return struct.pack('B', long)

def mulm(x, y, mod):
    return x * y % mod

def createRandomExponent():
    return random.getrandbits(192*8)

def sha256(message):
    return long(hashlib.sha256(str(message)).hexdigest(), 16)