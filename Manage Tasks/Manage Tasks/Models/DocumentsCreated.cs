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
    
    public partial class DocumentsCreated
    {
        public int ID { get; set; }
        public int FileSize { get; set; }
        public string FileName { get; set; }
        public byte[] Dokument { get; set; }
        public bool IsDeleted { get; set; }
        public System.DateTime CreatedOnDate { get; set; }
        public string LastModifiedByUserID { get; set; }
        public System.DateTime LastModifiedOnDate { get; set; }
        public Nullable<int> UserID { get; set; }
        public Nullable<int> TaskID { get; set; }
    
        public virtual TaskConfig TaskConfig { get; set; }
        public virtual UserProfile UserProfile { get; set; }
    }
}