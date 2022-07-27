using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProductKey
{
    public class ProductKey
    {
        #region budowa klucza
        /*
          ---------------------< BUDOWA >-----------------------
          [1-5] losowe
          [6-10] losowe
          [11-15] losowe
          [16] 
          [17] 
          [18] 
          [19] losowe
          [20] max users: ([19] + losowe) % 26
          [21] crc [1-5]; CRC = ([1] + [2] + 7*[3] + [4] + [5]) % 26
          [22] crc [6-10]; CRC = ([6] + 5*[7] + [8] + [9] + [10]) % 26 
          [23] crc [11-15]; CRC = ([11] + [12] + [13] + [14] + 3*[15]) % 26
          [24] crc [16-20]; CRC = (2*[16] + [17] + [18] + [19] + [20]) % 26
          [25] crc [21-24]; CRC = ([21] + [22] + [23] + [24]) % 26
        */
        #endregion

        private readonly char[] tab = "ABCDEFGHIJKLMNOPQRSTUVWXYZ".ToArray();
        private readonly int[] parameters = new int[] { 7, 5, 3, 2 };

        private string key;
        private Random rnd;
        public string Key
        {
            get
            {
                return key;
            }

            private set
            {
                key = value;
            }
        }

        public ProductKey(string _Key)
        {
            key = _Key.Replace('-', string.Empty.ToCharArray()[0]);
            rnd = new Random((int)DateTime.Now.Ticks);
        }

        public ProductKey(ProductKey _Key)
        {
            key = _Key.Key;
            rnd = new Random((int)DateTime.Now.Ticks);
        }

        public ProductKey()
        {
            rnd = new Random((int)DateTime.Now.Ticks);
        }

        private int charToInt(char sign)
        {
            return ((int)sign - 65);
        }

        public bool generateKey(int maxUsers = 5)
        {
            if (maxUsers < 1) return false;
            if (maxUsers > 100) return false;
            int div = (maxUsers / 26) % 26;
            int rem = maxUsers % 26;
            StringBuilder s = new StringBuilder(25);
            try
            {
                for (int i = 0; i < 15; i++)
                {
                    s.Append(tab[rnd.Next(26)]); //0-14
                }

                s.Append(tab[rnd.Next(26)]); //15
                s.Append(tab[rnd.Next(26)]); //16
                s.Append(tab[rnd.Next(26)]); //17
                if (charToInt(s[16]) > div) s.Append(tab[(26 - charToInt(s[16])) + div]); //18
                else s.Append(tab[div - charToInt(s[16])]);
                if (charToInt(s[17]) > rem) s.Append(tab[((26 - charToInt(s[17])) + rem)]); //19
                else s.Append(tab[rem - charToInt(s[17])]);

                s.Append(tab[(charToInt(s[0]) + charToInt(s[1]) + parameters[0] * charToInt(s[2]) + charToInt(s[3]) + charToInt(s[4])) % 26]); //20
                s.Append(tab[(charToInt(s[5]) + parameters[1] * charToInt(s[6]) + charToInt(s[7]) + charToInt(s[8]) + charToInt(s[9])) % 26]); //21
                s.Append(tab[(charToInt(s[10]) + charToInt(s[11]) + charToInt(s[12]) + charToInt(s[13]) + parameters[2] * charToInt(s[14])) % 26]); //22
                s.Append(tab[(parameters[3] * charToInt(s[15]) + charToInt(s[16]) + charToInt(s[17]) + charToInt(s[18]) + charToInt(s[19])) % 26]); //23
                s.Append(tab[(charToInt(s[20]) + charToInt(s[21]) + charToInt(s[22]) + charToInt(s[23])) % 26]); //24

                Key = s.ToString();
                return true;
            }
            catch(Exception)
            {
                return false;
            }
            
        }

        public bool verifyKey()
        {
            Key = Key.Replace("-", String.Empty);
            if (key.Length != 25) return false;
            Console.WriteLine(key);
            // [0-4] == [20]
            int count = (charToInt(Key[0]) + charToInt(Key[1]) + parameters[0] * charToInt(Key[2]) + charToInt(Key[3]) + charToInt(Key[4])) % 26;
            if (charToInt(Key[20]) != count) return false;

            // [5-9] == [21]
            count = (charToInt(Key[5]) + parameters[1] * charToInt(Key[6]) + charToInt(Key[7]) + charToInt(Key[8]) + charToInt(Key[9])) % 26;
            if (charToInt(Key[21]) != count) return false;

            // [10-14] == [22]
            count = (charToInt(Key[10]) + charToInt(Key[11]) + charToInt(Key[12]) + charToInt(Key[13]) + parameters[2] * charToInt(Key[14])) % 26;
            if (charToInt(Key[22]) != count) return false;

            // [15-19] == [23]
            count = (parameters[3] * charToInt(Key[15]) + charToInt(Key[16]) + charToInt(Key[17]) + charToInt(Key[18]) + charToInt(Key[19])) % 26;
            if (charToInt(Key[23]) != count) return false;

            // [20-23] == [24]
            count = (charToInt(Key[20]) + charToInt(Key[21]) + charToInt(Key[22]) + charToInt(Key[23])) % 26;
            if (charToInt(Key[24]) != count) return false;

            return true;
        }

        public int checkMaxUsers()
        {
            return ((charToInt(Key[16]) + charToInt(Key[18])) % 26) * 26 + (charToInt(Key[17]) + charToInt(Key[19])) % 26;
        }

        public override string ToString()
        {
            return Key.Substring(0, 5) + '-' + Key.Substring(5, 5) + '-' + Key.Substring(10, 5) + '-' + Key.Substring(15, 5) + '-' + Key.Substring(20, 5);
        }
    }
}
