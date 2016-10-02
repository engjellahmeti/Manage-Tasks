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
    
    public partial class UserProfile
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public UserProfile()
        {
            this.DocumentsCreated = new HashSet<DocumentsCreated>();
            this.Image = new HashSet<Image>();
            this.TaskCreatorUser = new HashSet<TaskCreatorUser>();
            this.UserRole = new HashSet<UserRole>();
        }
    
        public int UserID { get; set; }
        public string Name { get; set; }
        public string Surname { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public string Username { get; set; }
        public string JobTitle { get; set; }
        public string MainResponsibility { get; set; }
        public string EducationCretification { get; set; }
        public bool IsDeleted { get; set; }
        public System.DateTime LastModifiedOnDate { get; set; }
        public System.DateTime CreatedOnDate { get; set; }
        public string LastModifiedByUserID { get; set; }
        public bool AcceptedFromSuperAdmin { get; set; }
        public Nullable<int> GroupID { get; set; }
        public string UsID { get; set; }
    
        public virtual AspNetUsers AspNetUsers { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<DocumentsCreated> DocumentsCreated { get; set; }
        public virtual Group Group { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Image> Image { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<TaskCreatorUser> TaskCreatorUser { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<UserRole> UserRole { get; set; }
    }
}
