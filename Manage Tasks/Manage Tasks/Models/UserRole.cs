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
    
    public partial class UserRole
    {
        public int ID { get; set; }
        public int UserID { get; set; }
        public string RoleID { get; set; }
    
        public virtual AspNetRoles AspNetRoles { get; set; }
        public virtual UserProfile UserProfile { get; set; }
    }
}
