using System.Collections.Generic;
using System.Numerics;

namespace PaillierVoting
{
    class QuizInput : Input
    {
        public QuizInput()
        {
            p = 1117;
            q = 1471;
            g = 652534095028;
            votes = new List<BigInteger>
            {
                new BigInteger(2074201421753),
                new BigInteger(312370494462),
                new BigInteger(780111187291),
                new BigInteger(1163439426584),
                new BigInteger(2677246222224),
                new BigInteger(86389379151),
                new BigInteger(298872530125),
                new BigInteger(2053140965009),
                new BigInteger(1914975248946),
                new BigInteger(2062450717664),
                new BigInteger(2602646419292),
                new BigInteger(1188313432502),
                new BigInteger(1460596308618),
                new BigInteger(1816551900732),
                new BigInteger(130371414466),
                new BigInteger(2082974605067),
                new BigInteger(1636620384026),
                new BigInteger(1096672550297),
                new BigInteger(817672318147),
                new BigInteger(812257162396),
                new BigInteger(979549876813),
                new BigInteger(1857648187791),
                new BigInteger(2694939373963),
                new BigInteger(1837053575762),
                new BigInteger(511750975369),
                new BigInteger(532605060654),
                new BigInteger(736291830187),
                new BigInteger(1352363583326),
                new BigInteger(1177945671895),
                new BigInteger(1623847905760),
                new BigInteger(2611556449248),
                new BigInteger(2345267855078),
                new BigInteger(2613256198870),
                new BigInteger(1944258247565),
                new BigInteger(1010040551645),
                new BigInteger(1848151167660),
                new BigInteger(2094990581481),
                new BigInteger(1982809992380),
                new BigInteger(1481510150362),
                new BigInteger(296926832621),
                new BigInteger(2238547172316),
                new BigInteger(2400666563531),
                new BigInteger(1780649962800),
                new BigInteger(611453607583),
                new BigInteger(1978092659483),
                new BigInteger(87801486323),
                new BigInteger(916159774066),
                new BigInteger(1152891673539),
                new BigInteger(2047149314),
            };
        }

        public BigInteger p { get; set; }
        public BigInteger q { get; set; }
        public BigInteger g { get; set; }
        public List<BigInteger> votes { get; set; }
    }
}
