//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace Manage_Tasks.Models
{
    using System;
    using System.Collections.Generic;
    
    public partial class RequestTime
    {
        public int ID { get; set; }
        public string PersonWhoRequested { get; set; }
        public System.DateTime DateRequired { get; set; }
        public System.DateTime DateRequested { get; set; }
        public Nullable<bool> IsAccepted { get; set; }
        public int TaskID { get; set; }
    
        public virtual TaskConfig TaskConfig { get; set; }
    }
}
