using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Byzantium
{
    //Handles some bitwise error operations.
    static class NodeErrors
    {
        public const int NO_NETWORK_ADAPTORS = 0x1;
        public const int NETWORK_ADAPTOR_UNCHOSEN = 0x2;
        public const int PORTS_UNCHOSEN = 0x4;
        public static string getStringErr(int int_err)
        {
            string str_err = "N/A";
            switch (int_err)
            {
                case NO_NETWORK_ADAPTORS:
                    str_err = "No Network Adapators";
                    break;
                case NETWORK_ADAPTOR_UNCHOSEN:
                    str_err = "Multiple Host Addresses- Choose One";
                    break;
                case PORTS_UNCHOSEN:
                    str_err = "Ports Unchosen- Choose Port Numbers";
                    break;
            }
            return str_err;
        }

        public static bool hasErr(int state, int err_code)
        {
            return (state & err_code) != 0;
        }

        public static int toggleErr(int state, int err_code)
        {
            return state ^ err_code;
        }

    }
}
