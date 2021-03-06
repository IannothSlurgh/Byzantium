﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Collections;

namespace Byzantium
{
    //A fixed capacity array of IpAddresses
    //in the same subnet which are sorted
    //from least to greatest.
    public class SortedAddressList : IEnumerable
    {
        private List<IPAddress> addresses;
        private int capacity;
        private int len;


        public SortedAddressList(int _capacity)
        {
            if (_capacity < 0)
            {
                _capacity = 0;
            }
            addresses = new List<IPAddress>(_capacity);
            capacity = _capacity;
            len = 0;
        }

        private static bool greater(IPAddress one, IPAddress two)
        {
            return one.GetAddressBytes()[3] > two.GetAddressBytes()[3];
        }

        private static bool lesser(IPAddress one, IPAddress two)
        {
            return one.GetAddressBytes()[3] < two.GetAddressBytes()[3];
        }

        class ipcomparer : IComparer<IPAddress>
        {
            public int Compare(IPAddress one, IPAddress two)
            {
                if (greater(one, two))
                    return 1;
                if (lesser(one, two))
                    return -1;
                return 0;
            }
        }

        //Return an index max for the for loop of binary search.
        /*private int log_2(int num)
        {
            int times = 0;
            while (num > 2)
            {
                num /= 2;
                times++;
            }
            return times;
        }*/

        //Use binary search to look for ipaddress in list.
        //Could have used BinarySearch<t>, was oversight.
        /*private bool find(IPAddress ip)
        {
            //Empty means not inside.
            if (len == 0)
            {
                return false;
            }
            int max_times = log_2(len);
            int mid = len / 2;
            int bottom = 0;
            int top = len;
            for (int i = 0; i <= max_times; ++i)
            {
                int temp = mid;
                if (lesser(ip, addresses[mid]))
                {
                    mid = (mid + bottom) / 2;
                    if (mid == temp)
                    {
                        return false;
                    }
                    top = temp;
                }
                else
                {
                    if (greater(ip, addresses[mid]))
                    {
                        mid = (mid + top) / 2;
                        if (mid == temp)
                        {
                            return false;
                        }
                        bottom = temp+1;
                    }
                    else
                    {
                        return true;
                    }
                }
            }
            return false;
        }*/

        public bool find(IPAddress searched_for)
        {
            int loc = addresses.BinarySearch(searched_for, new ipcomparer());
            if (loc < 0)
            {
                return false;
            }
            return true;
        }

        public bool add(byte[] ip_bytes)
        {
            IPAddress ip;
            try
            {
                ip = new IPAddress(ip_bytes);
            }
            catch(ArgumentException e)
            {
                return false;
            }
            return add(ip);
        }

        public bool add(String addr)
        {
            IPAddress ip;
            try
            {
                ip = IPAddress.Parse(addr);
            }
            catch(FormatException e)
            {
                return false;
            }
            return add(ip);
        }

        public bool add(IPAddress a)
        {
            //Fixed size, never exceed capacity.
            if (len + 1 > capacity)
            {
                return false;
            }
            //Empty requires no sorting.
            if (len == 0)
            {
                len += 1;
                addresses.Add(a);
                return true;
            }
            //We want unique addrs only.
            if (addresses.BinarySearch(a, new ipcomparer()) >= 0)
            {
                return false;
            }
            len += 1;
            addresses.Add(a);
            addresses.Sort(new ipcomparer());
            return true;
        }

        //Read-only indexer.
        public IPAddress this[int index]
        {
            get
            {
                return addresses[index];
            }
        }

        public bool remove(IPAddress a)
        {
            int loc = addresses.BinarySearch(a, new ipcomparer());
            if (loc < 0)
                return false;
            //No sorting needed. Removing an item will maintain order.
            addresses.RemoveAt(loc);
            return true;
        }

        public int length()
        {
            return len;
        }

        public string ToString()
        {
            string concat = "";
            foreach (IPAddress ip in addresses)
            {
                concat += ip.ToString();
                concat += " ";
            }
            return concat;
        }

        private class SortedAddressListEnumerator : IEnumerator
        {
            List<IPAddress> my_list;
            int iterator_index = 0;
            int len = 0;

            public SortedAddressListEnumerator(ref List<IPAddress> _my_list, int _len) 
            {
                my_list = _my_list;
                len = _len;
            }

            public bool MoveNext()
            {
                iterator_index++;
                return (iterator_index < len);
            }

            public void Reset()
            { iterator_index = 0; }

            public object Current
            {
                get
                {
                    try
                    {
                        return my_list[iterator_index];
                    }
                    catch (IndexOutOfRangeException)
                    {
                        throw new InvalidOperationException();
                    }
                }
            }
        }
        public IEnumerator GetEnumerator()
        {
            return (IEnumerator)new SortedAddressListEnumerator(ref addresses, len);
        }

    }
}
