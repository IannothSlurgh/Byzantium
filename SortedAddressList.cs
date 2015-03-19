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
    class SortedAddressList
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
            last_location = 0;
        }

        private bool greater(IPAddress one, IPAddress two)
        {
            return one.GetAddressBytes()[3] > two.GetAddressBytes()[3];
        }

        private bool lesser(IPAddress one, IPAddress two)
        {
            return one.GetAddressBytes()[3] < two.GetAddressBytes()[3];
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

        struct find_struct
        {
            //Was this IPAddress in the sorted list?
            public bool found;
            //If it was found, at what index?
            //If not, what was the last index search tried?
            public int last_location;
        }

        //Use binary search to look for ipaddress in list.
        private find_struct find_actual(IPAddress ip)
        {
            find_struct result = new find_struct();
            result.found = false;
            //Empty means not inside.
            if (length == 0)
            {
                result.found = false;
                return result;
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
                        //False.
                        return result;
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
                            //false
                            return result;
                        }
                        bottom = temp+1;
                    }
                    else
                    {
                        result.last_location = mid;
                        result.found = true;
                        return result;
                    }
                }
            }
            return result;
        }

        //Used by outsiders to binary search for an IP.
        public bool find(IPAddress ip)
        {
            return find_actual(ip).found;
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
                addresses.Add(a);
                return true;
            }
            find_struct find_result = find_actual(a);
            if (find_result.found)
            {
                return false;
            }
            else
            {

            }
            return false;
        }
    }
}
