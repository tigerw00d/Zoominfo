namespace Darkmoon_Bruteforce_Engine
{
    using System;

    internal class GetProxy_Tools
    {
        public static GetType GT = GetType.NONE;
        private static int numprx = 0;

        public static int GetProxy(int d)
        {
            if (numprx >= d)
            {
                numprx = 0;
            }
            numprx++;
            return (numprx - 1);
        }

        public new enum GetType
        {
            HTTP,
            SOCKS4,
            SOCKS5,
            NONE
        }
    }
}

