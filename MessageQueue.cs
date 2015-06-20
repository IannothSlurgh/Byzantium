using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Byzantium
{
    struct Message
    {
        public byte[] addr;
        public string msg;
        public string proto;
        public bool is_bad;
    }
    //Note to self, make Message struct to allow only one List.
    class MessageQueue
    {
        private LinkedList<Message> messages = new LinkedList<Message>();
        private int capacity;
        private int length;
        Boolean unique_addr;
        public MessageQueue(int _capacity)
        {
            capacity = _capacity;
            unique_addr = false;
            length = 0;
        }

        public MessageQueue(int _capacity, Boolean _unique_addr):this(_capacity)
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

        public bool Enqueue(byte[] _addr, string _msg, string _proto)
        {
            Message new_msg;
            new_msg.msg = _msg;
            new_msg.addr = _addr;
            new_msg.proto = _proto;
            new_msg.is_bad = false;
            return Enqueue(new_msg);
        }

        public bool Enqueue(Message _new_msg)
        {
            //Never exceed user specified capacity.
            if (length + 1 > capacity)
            {
                return false;
            }
            //No "null messages"
            if (_new_msg.msg.Length == 0 || _new_msg.msg == null)
            {
                return false;
            }
            foreach (Message possible_msg in messages)
            {
                if (eq_addr(_new_msg.addr, possible_msg.addr))
                 {
                        //A user may specify that there is only
                        //one message per address.
                        if (unique_addr)
                        {
                            return false;
                        }
                        else
                        {
                            //If user did not specify only one message
                            //Per address, Only ignore duplicate messages
                            //from a particular address.
                            if (possible_msg.msg == _new_msg.msg)
                            {
                                return false;
                            }
                        }
                  }
            }
            //If the rules have been obeyed- we have space,
            //we are not adding a duplicate message from the same source,
            //If we specified only one message from a source, the address
            //must not have appeared in the container- then add new element.
            length += 1;
            messages.AddLast(_new_msg);
            return true;
        }

        public Message Dequeue()
        {
            if (length == 0)
            {
                Message bad_msg;
                bad_msg.addr = null;
                bad_msg.proto = null;
                bad_msg.msg = null;
                bad_msg.is_bad = true;
                return bad_msg;
            }
            length -= 1;
            Message next_msg = messages.First();
            messages.RemoveFirst();
            return next_msg;
        }

        public int Length()
        {
            return length;
        }

        public int Capacity()
        {
            return capacity;
        }
    }
}
