using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;

namespace Byzantium
{
    //A fixed capacity array of IpAddresses
    //in the same subnet which are sorted
    //from least to greatest.
    public class SortedAddressList
    {
        private List<IPAddress> addresses;
        private int capacity;
        private int length;

        public SortedAddressList(int _capacity)
        {
            if (_capacity < 0)
            {
                _capacity = 0;
            }
            addresses = new List<IPAddress>(_capacity);
            capacity = _capacity;
            length = 0;
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
        private int log_2(int num)
        {
            int times = 0;
            while (num > 2)
            {
                num /= 2;
                times++;
            }
            return times;
        }

        //Use binary search to look for ipaddress in list.
        //Could have used BinarySearch<t>, was oversight.
        private bool find(IPAddress ip)
        {
            //Empty means not inside.
            if (length == 0)
            {
                return false;
            }
            int max_times = log_2(length);
            int mid = length / 2;
            int bottom = 0;
            int top = length;
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
        }

        public bool add(IPAddress a)
        {
            //Fixed size, never exceed capacity.
            if (length + 1 > capacity)
            {
                return false;
            }
            //Empty requires no sorting.
            if (length == 0)
            {
                length += 1;
                addresses.Add(a);
                return true;
            }
            //We want unique addrs only.
            if (addresses.BinarySearch(a, new ipcomparer()) >= 0)
            {
                return false;
            }
            length += 1;
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
    }
}
