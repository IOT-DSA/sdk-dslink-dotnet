using System;
using System.Collections.Generic;
using System.Text;
using DSLink;

/* 
 * This class represents the full set of DSLink Command Line Options
 */

namespace DSLink
{
    public class DSLinkOptions
    {
        public DSLinkOptions(string LinkName)
        {
        }
      
        public string BrokerUrl { get; set; }
        public string BrokerToken { get; set; }
        public string LinkName { get; set; }
        public string Key { get; set; }
        public string DSLinkJsonPath { get; set; }
        public string NodesFilename { get; set; }
    }
}
