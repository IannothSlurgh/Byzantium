using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Byzantium
{
    //Note to self, make Message struct to allow only one List.
    class MessageQueue
    {
        private List<string> messages = new List<string>();
        private List<byte[]> addresses = new List<byte[]>();
        private int capacity;
        private int length;
        Boolean unique_addr;
        MessageQueue(int _capacity)
        {
            capacity = _capacity;
            messages.Capacity = capacity;
            addresses.Capacity = capacity;
            unique_addr = false;
            length = 0;
        }

        MessageQueue(int _capacity, Boolean _unique_addr):this(_capacity)
        {
            unique_addr = _unique_addr;
        }

        private bool eq_addr(byte[] one, byte[] two)
        {
            if (one.Length != two.Length)
            {
                return false;
            }
            for (int i = 0; i < one.Length;++i )
            {
                if (one[i] != two[i])
                {
                    return false;
                }
            }
            return true;
        }

        public bool add(string msg, byte[] addr)
        {
            //Never exceed user specified capacity.
            if (length + 1 > capacity)
            {
                return false;
            }
            //A user may specify that there is only
            //one message per address.
            if (unique_addr)
            {
                for (int i = 0; i < length; ++i)
                {
                    if (eq_addr(addr, addresses[i]))
                    {
                        return false;
                    }
                }
            }
            else
            {
                //If user did not specify only one message
                //Per address, Only ignore duplicate messages
                //from a particular address.
                for (int i = 0; i < length; ++i)
                {
                    if (messages[i] == msg && eq_addr(addr, addresses[i]))
                    {
                        return false;
                    }
                }
            }
            //If the rules have been obeyed- we have space,
            //we are not adding a duplicate message from the same source,
            //If we specified only one message from a source, the address
            //must not have appeared in the container- then add new element.
            length += 1;
            messages.Add(msg);
            addresses.Add(addr);
            return true;
        }

    }
}
