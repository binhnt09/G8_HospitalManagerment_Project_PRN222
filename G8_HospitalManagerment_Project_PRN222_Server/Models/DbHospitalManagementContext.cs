using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace G8_HospitalManagerment_Project_PRN222_Server.Models;

public partial class DbHospitalManagementContext : DbContext
{
    public DbHospitalManagementContext()
    {
    }

    public DbHospitalManagementContext(DbContextOptions<DbHospitalManagementContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Appointment> Appointments { get; set; }

    public virtual DbSet<Authentication> Authentications { get; set; }

    public virtual DbSet<Bed> Beds { get; set; }

    public virtual DbSet<DailyCareRecord> DailyCareRecords { get; set; }

    public virtual DbSet<Department> Departments { get; set; }

    public virtual DbSet<Doctor> Doctors { get; set; }

    public virtual DbSet<Drug> Drugs { get; set; }

    public virtual DbSet<Employee> Employees { get; set; }

    public virtual DbSet<ImagingOrder> ImagingOrders { get; set; }

    public virtual DbSet<ImagingResult> ImagingResults { get; set; }

    public virtual DbSet<InpatientAdmission> InpatientAdmissions { get; set; }

    public virtual DbSet<Invoice> Invoices { get; set; }

    public virtual DbSet<InvoiceItem> InvoiceItems { get; set; }

    public virtual DbSet<LabOrder> LabOrders { get; set; }

    public virtual DbSet<LabOrderItem> LabOrderItems { get; set; }

    public virtual DbSet<LabResult> LabResults { get; set; }

    public virtual DbSet<MedicalRecord> MedicalRecords { get; set; }

    public virtual DbSet<Notification> Notifications { get; set; }

    public virtual DbSet<OperationRoom> OperationRooms { get; set; }

    public virtual DbSet<Patient> Patients { get; set; }

    public virtual DbSet<Payment> Payments { get; set; }

    public virtual DbSet<Prescription> Prescriptions { get; set; }

    public virtual DbSet<PrescriptionItem> PrescriptionItems { get; set; }

    public virtual DbSet<Room> Rooms { get; set; }

    public virtual DbSet<Service> Services { get; set; }

    public virtual DbSet<SurgeryRecord> SurgeryRecords { get; set; }

    public virtual DbSet<SurgerySchedule> SurgerySchedules { get; set; }

    public virtual DbSet<Test> Tests { get; set; }

    public virtual DbSet<User> Users { get; set; }

    public virtual DbSet<UserRole> UserRoles { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        => optionsBuilder.UseSqlServer("Name=ConnectionStrings:MyCnn");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Appointment>(entity =>
        {
            entity.HasKey(e => e.AppointmentId).HasName("PK__Appointm__8ECDFCA2D2B791E8");

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(getdate())");
            entity.Property(e => e.IsDeleted).HasDefaultValue(false);
            entity.Property(e => e.UpdatedAt).HasDefaultValueSql("(getdate())");

            entity.HasOne(d => d.Department).WithMany(p => p.Appointments).HasConstraintName("FK__Appointme__Depar__656C112C");

            entity.HasOne(d => d.Doctor).WithMany(p => p.Appointments).HasConstraintName("FK__Appointme__Docto__6477ECF3");

            entity.HasOne(d => d.Patient).WithMany(p => p.Appointments)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Appointme__Patie__6383C8BA");
        });

        modelBuilder.Entity<Authentication>(entity =>
        {
            entity.HasKey(e => e.AuthenticationId).HasName("PK__Authenti__81919C5B21E18E3F");

            entity.Property(e => e.AuthType).HasDefaultValue("local");
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(getdate())");
            entity.Property(e => e.UpdatedAt).HasDefaultValueSql("(getdate())");

            entity.HasOne(d => d.User).WithMany(p => p.Authentications)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Authentic__UserI__4316F928");
        });

        modelBuilder.Entity<Bed>(entity =>
        {
            entity.HasKey(e => e.BedId).HasName("PK__Beds__A8A710602211C283");

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(getdate())");
            entity.Property(e => e.IsDeleted).HasDefaultValue(false);
            entity.Property(e => e.IsOccupied).HasDefaultValue(false);
            entity.Property(e => e.UpdatedAt).HasDefaultValueSql("(getdate())");

            entity.HasOne(d => d.Room).WithMany(p => p.Beds).HasConstraintName("FK__Beds__RoomID__607251E5");
        });

        modelBuilder.Entity<DailyCareRecord>(entity =>
        {
            entity.HasKey(e => e.CareId).HasName("PK__DailyCar__544993E7709EC0F0");

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(getdate())");
            entity.Property(e => e.IsDeleted).HasDefaultValue(false);
            entity.Property(e => e.RecordDate).HasDefaultValueSql("(getdate())");
            entity.Property(e => e.UpdatedAt).HasDefaultValueSql("(getdate())");

            entity.HasOne(d => d.Admission).WithMany(p => p.DailyCareRecords).HasConstraintName("FK__DailyCare__Admis__70A8B9AE");

            entity.HasOne(d => d.Employee).WithMany(p => p.DailyCareRecords).HasConstraintName("FK__DailyCare__Emplo__719CDDE7");
        });

        modelBuilder.Entity<Department>(entity =>
        {
            entity.HasKey(e => e.DepartmentId).HasName("PK__Departme__B2079BCD1C6487AA");

            entity.HasIndex(e => e.Phone, "IX_Department_Phone")
                .IsUnique()
                .HasFilter("([Phone] IS NOT NULL)");

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(getdate())");
            entity.Property(e => e.IsDeleted).HasDefaultValue(false);
            entity.Property(e => e.UpdatedAt).HasDefaultValueSql("(getdate())");
        });

        modelBuilder.Entity<Doctor>(entity =>
        {
            entity.HasKey(e => e.DoctorId).HasName("PK__Doctors__2DC00EDFC8D9CEBA");

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(getdate())");
            entity.Property(e => e.IsDeleted).HasDefaultValue(false);
            entity.Property(e => e.UpdatedAt).HasDefaultValueSql("(getdate())");

            entity.HasOne(d => d.Employee).WithOne(p => p.Doctor)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Doctors__Employe__5812160E");
        });

        modelBuilder.Entity<Drug>(entity =>
        {
            entity.HasKey(e => e.DrugId).HasName("PK__Drugs__908D66F6D0484267");

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(getdate())");
            entity.Property(e => e.IsDeleted).HasDefaultValue(false);
            entity.Property(e => e.StockQuantity).HasDefaultValue(0);
            entity.Property(e => e.UpdatedAt).HasDefaultValueSql("(getdate())");
        });

        modelBuilder.Entity<Employee>(entity =>
        {
            entity.HasKey(e => e.EmployeeId).HasName("PK__Employee__7AD04FF1B085A4A9");

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(getdate())");
            entity.Property(e => e.HireDate).HasDefaultValueSql("(getdate())");
            entity.Property(e => e.IsDeleted).HasDefaultValue(false);
            entity.Property(e => e.UpdatedAt).HasDefaultValueSql("(getdate())");
            entity.Property(e => e.WorkStatus).HasDefaultValue("Active");

            entity.HasOne(d => d.Department).WithMany(p => p.Employees)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Employees__Depar__5165187F");

            entity.HasOne(d => d.User).WithMany(p => p.Employees)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Employees__UserI__5070F446");
        });

        modelBuilder.Entity<ImagingOrder>(entity =>
        {
            entity.HasKey(e => e.OrderId).HasName("PK__ImagingO__C3905BAF938613E9");

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(getdate())");
            entity.Property(e => e.IsDeleted).HasDefaultValue(false);
            entity.Property(e => e.OrderDate).HasDefaultValueSql("(getdate())");
            entity.Property(e => e.Status).HasDefaultValue("Pending");
            entity.Property(e => e.UpdatedAt).HasDefaultValueSql("(getdate())");

            entity.HasOne(d => d.Doctor).WithMany(p => p.ImagingOrders)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__ImagingOr__Docto__4B7734FF");

            entity.HasOne(d => d.MedicalRecord).WithMany(p => p.ImagingOrders)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__ImagingOr__Medic__498EEC8D");

            entity.HasOne(d => d.Patient).WithMany(p => p.ImagingOrders)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__ImagingOr__Patie__4A8310C6");

            entity.HasOne(d => d.Service).WithMany(p => p.ImagingOrders)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__ImagingOr__Servi__4C6B5938");
        });

        modelBuilder.Entity<ImagingResult>(entity =>
        {
            entity.HasKey(e => e.ResultId).HasName("PK__ImagingR__97690228F9B46C6B");

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(getdate())");
            entity.Property(e => e.IsDeleted).HasDefaultValue(false);
            entity.Property(e => e.ResultDate).HasDefaultValueSql("(getdate())");
            entity.Property(e => e.UpdatedAt).HasDefaultValueSql("(getdate())");

            entity.HasOne(d => d.Order).WithMany(p => p.ImagingResults).HasConstraintName("FK__ImagingRe__Order__540C7B00");

            entity.HasOne(d => d.PerformedByNavigation).WithMany(p => p.ImagingResults).HasConstraintName("FK__ImagingRe__Perfo__55009F39");
        });

        modelBuilder.Entity<InpatientAdmission>(entity =>
        {
            entity.HasKey(e => e.AdmissionId).HasName("PK__Inpatien__C97EEFA2B6427F94");

            entity.Property(e => e.AdmissionDate).HasDefaultValueSql("(getdate())");
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(getdate())");
            entity.Property(e => e.IsDeleted).HasDefaultValue(false);
            entity.Property(e => e.Status).HasDefaultValue("Admitted");
            entity.Property(e => e.UpdatedAt).HasDefaultValueSql("(getdate())");

            entity.HasOne(d => d.Bed).WithMany(p => p.InpatientAdmissions).HasConstraintName("FK__Inpatient__BedID__690797E6");

            entity.HasOne(d => d.Doctor).WithMany(p => p.InpatientAdmissions).HasConstraintName("FK__Inpatient__Docto__681373AD");

            entity.HasOne(d => d.Patient).WithMany(p => p.InpatientAdmissions).HasConstraintName("FK__Inpatient__Patie__671F4F74");
        });

        modelBuilder.Entity<Invoice>(entity =>
        {
            entity.HasKey(e => e.InvoiceId).HasName("PK__Invoices__D796AAD5D56F27FC");

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(getdate())");
            entity.Property(e => e.Discount).HasDefaultValue(0m);
            entity.Property(e => e.IsDeleted).HasDefaultValue(false);
            entity.Property(e => e.IssueDate).HasDefaultValueSql("(getdate())");
            entity.Property(e => e.UpdatedAt).HasDefaultValueSql("(getdate())");

            entity.HasOne(d => d.Appointment).WithMany(p => p.Invoices).HasConstraintName("FK__Invoices__Appoin__2CF2ADDF");

            entity.HasOne(d => d.Patient).WithMany(p => p.Invoices)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Invoices__Patien__2BFE89A6");
        });

        modelBuilder.Entity<InvoiceItem>(entity =>
        {
            entity.HasKey(e => e.InvoiceItemId).HasName("PK__InvoiceI__478FE0FC12F19FAB");

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(getdate())");
            entity.Property(e => e.IsDeleted).HasDefaultValue(false);
            entity.Property(e => e.Quantity).HasDefaultValue(1);
            entity.Property(e => e.UpdatedAt).HasDefaultValueSql("(getdate())");

            entity.HasOne(d => d.Invoice).WithMany(p => p.InvoiceItems).HasConstraintName("FK__InvoiceIt__Invoi__367C1819");
        });

        modelBuilder.Entity<LabOrder>(entity =>
        {
            entity.HasKey(e => e.OrderId).HasName("PK__LabOrder__C3905BAF9A0231E7");

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(getdate())");
            entity.Property(e => e.IsDeleted).HasDefaultValue(false);
            entity.Property(e => e.OrderDate).HasDefaultValueSql("(getdate())");
            entity.Property(e => e.Status).HasDefaultValue("Pending");
            entity.Property(e => e.UpdatedAt).HasDefaultValueSql("(getdate())");

            entity.HasOne(d => d.Doctor).WithMany(p => p.LabOrders)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__LabOrders__Docto__7B5B524B");

            entity.HasOne(d => d.MedicalRecord).WithMany(p => p.LabOrders)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__LabOrders__Medic__797309D9");

            entity.HasOne(d => d.Patient).WithMany(p => p.LabOrders)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__LabOrders__Patie__7A672E12");
        });

        modelBuilder.Entity<LabOrderItem>(entity =>
        {
            entity.HasKey(e => e.OrderItemId).HasName("PK__LabOrder__57ED06A1E0AF5977");

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(getdate())");
            entity.Property(e => e.IsDeleted).HasDefaultValue(false);
            entity.Property(e => e.Status).HasDefaultValue("Pending");
            entity.Property(e => e.UpdatedAt).HasDefaultValueSql("(getdate())");

            entity.HasOne(d => d.Order).WithMany(p => p.LabOrderItems).HasConstraintName("FK__LabOrderI__Order__03F0984C");

            entity.HasOne(d => d.Test).WithMany(p => p.LabOrderItems)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__LabOrderI__TestI__04E4BC85");
        });

        modelBuilder.Entity<LabResult>(entity =>
        {
            entity.HasKey(e => e.ResultId).HasName("PK__LabResul__976902284F1092D0");

            entity.Property(e => e.IsAbnormal).HasDefaultValue(false);
            entity.Property(e => e.ResultDate).HasDefaultValueSql("(getdate())");

            entity.HasOne(d => d.OrderItem).WithMany(p => p.LabResults).HasConstraintName("FK__LabResult__Order__0C85DE4D");

            entity.HasOne(d => d.PerformedByNavigation).WithMany(p => p.LabResults).HasConstraintName("FK__LabResult__Perfo__0E6E26BF");
        });

        modelBuilder.Entity<MedicalRecord>(entity =>
        {
            entity.HasKey(e => e.RecordId).HasName("PK__MedicalR__FBDF78C950114167");

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(getdate())");
            entity.Property(e => e.IsDeleted).HasDefaultValue(false);
            entity.Property(e => e.RecordDate).HasDefaultValueSql("(getdate())");
            entity.Property(e => e.UpdatedAt).HasDefaultValueSql("(getdate())");

            entity.HasOne(d => d.Appointment).WithMany(p => p.MedicalRecords).HasConstraintName("FK__MedicalRe__Appoi__6C190EBB");

            entity.HasOne(d => d.Doctor).WithMany(p => p.MedicalRecords)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__MedicalRe__Docto__6E01572D");

            entity.HasOne(d => d.Patient).WithMany(p => p.MedicalRecords)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__MedicalRe__Patie__6D0D32F4");
        });

        modelBuilder.Entity<Notification>(entity =>
        {
            entity.HasKey(e => e.NotificationId).HasName("PK__Notifica__20CF2E3264842AE3");

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(getdate())");
            entity.Property(e => e.IsDeleted).HasDefaultValue(false);
            entity.Property(e => e.IsRead).HasDefaultValue(false);
            entity.Property(e => e.UpdatedAt).HasDefaultValueSql("(getdate())");

            entity.HasOne(d => d.User).WithMany(p => p.Notifications)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Notificat__UserI__42E1EEFE");
        });

        modelBuilder.Entity<OperationRoom>(entity =>
        {
            entity.HasKey(e => e.OroomId).HasName("PK__Operatio__BFD0496D944C3992");

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(getdate())");
            entity.Property(e => e.IsDeleted).HasDefaultValue(false);
            entity.Property(e => e.Status).HasDefaultValue("Available");
            entity.Property(e => e.UpdatedAt).HasDefaultValueSql("(getdate())");
        });

        modelBuilder.Entity<Patient>(entity =>
        {
            entity.HasKey(e => e.PatientId).HasName("PK__Patients__970EC34674AF05DC");

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(getdate())");
            entity.Property(e => e.IsDeleted).HasDefaultValue(false);
            entity.Property(e => e.UpdatedAt).HasDefaultValueSql("(getdate())");

            entity.HasOne(d => d.User).WithMany(p => p.Patients)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Patients__UserID__5DCAEF64");
        });

        modelBuilder.Entity<Payment>(entity =>
        {
            entity.HasKey(e => e.PaymentId).HasName("PK__Payments__9B556A58D77B3A0A");

            entity.Property(e => e.IsDeleted).HasDefaultValue(false);
            entity.Property(e => e.PaymentDate).HasDefaultValueSql("(getdate())");
            entity.Property(e => e.UpdatedAt).HasDefaultValueSql("(getdate())");

            entity.HasOne(d => d.Invoice).WithMany(p => p.Payments)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Payments__Invoic__3D2915A8");
        });

        modelBuilder.Entity<Prescription>(entity =>
        {
            entity.HasKey(e => e.PrescriptionId).HasName("PK__Prescrip__40130812280786E0");

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(getdate())");
            entity.Property(e => e.IsDeleted).HasDefaultValue(false);
            entity.Property(e => e.PrescriptionDate).HasDefaultValueSql("(getdate())");
            entity.Property(e => e.UpdatedAt).HasDefaultValueSql("(getdate())");

            entity.HasOne(d => d.Doctor).WithMany(p => p.Prescriptions)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Prescript__Docto__19DFD96B");

            entity.HasOne(d => d.MedicalRecord).WithMany(p => p.Prescriptions)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Prescript__Medic__17F790F9");

            entity.HasOne(d => d.Patient).WithMany(p => p.Prescriptions)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Prescript__Patie__18EBB532");
        });

        modelBuilder.Entity<PrescriptionItem>(entity =>
        {
            entity.HasKey(e => e.PrescriptionItemId).HasName("PK__Prescrip__1AADD9DA9594DEC7");

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(getdate())");
            entity.Property(e => e.IsDeleted).HasDefaultValue(false);
            entity.Property(e => e.UpdatedAt).HasDefaultValueSql("(getdate())");

            entity.HasOne(d => d.Drug).WithMany(p => p.PrescriptionItems)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Prescript__DrugI__2180FB33");

            entity.HasOne(d => d.Prescription).WithMany(p => p.PrescriptionItems).HasConstraintName("FK__Prescript__Presc__208CD6FA");
        });

        modelBuilder.Entity<Room>(entity =>
        {
            entity.HasKey(e => e.RoomId).HasName("PK__Rooms__32863919BB3FCAB4");

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(getdate())");
            entity.Property(e => e.IsDeleted).HasDefaultValue(false);
            entity.Property(e => e.UpdatedAt).HasDefaultValueSql("(getdate())");
        });

        modelBuilder.Entity<Service>(entity =>
        {
            entity.HasKey(e => e.ServiceId).HasName("PK__Services__C51BB0EA0A1D5661");

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(getdate())");
            entity.Property(e => e.IsDeleted).HasDefaultValue(false);
            entity.Property(e => e.UpdatedAt).HasDefaultValueSql("(getdate())");
        });

        modelBuilder.Entity<SurgeryRecord>(entity =>
        {
            entity.HasKey(e => e.RecordId).HasName("PK__SurgeryR__FBDF78C9EB9CD2B9");

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(getdate())");
            entity.Property(e => e.IsDeleted).HasDefaultValue(false);
            entity.Property(e => e.UpdatedAt).HasDefaultValueSql("(getdate())");

            entity.HasOne(d => d.Schedule).WithMany(p => p.SurgeryRecords).HasConstraintName("FK__SurgeryRe__Sched__078C1F06");
        });

        modelBuilder.Entity<SurgerySchedule>(entity =>
        {
            entity.HasKey(e => e.ScheduleId).HasName("PK__SurgeryS__9C8A5B69725DC444");

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(getdate())");
            entity.Property(e => e.IsDeleted).HasDefaultValue(false);
            entity.Property(e => e.Status).HasDefaultValue("Scheduled");
            entity.Property(e => e.UpdatedAt).HasDefaultValueSql("(getdate())");

            entity.HasOne(d => d.Anesthesiologist).WithMany(p => p.SurgeryScheduleAnesthesiologists).HasConstraintName("FK__SurgerySc__Anest__00DF2177");

            entity.HasOne(d => d.MainSurgeon).WithMany(p => p.SurgeryScheduleMainSurgeons).HasConstraintName("FK__SurgerySc__MainS__7FEAFD3E");

            entity.HasOne(d => d.Oroom).WithMany(p => p.SurgerySchedules).HasConstraintName("FK__SurgerySc__ORoom__7EF6D905");

            entity.HasOne(d => d.Patient).WithMany(p => p.SurgerySchedules).HasConstraintName("FK__SurgerySc__Patie__7E02B4CC");
        });

        modelBuilder.Entity<Test>(entity =>
        {
            entity.HasKey(e => e.TestId).HasName("PK__Tests__8CC33100208962B8");

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(getdate())");
            entity.Property(e => e.IsDeleted).HasDefaultValue(false);
            entity.Property(e => e.UpdatedAt).HasDefaultValueSql("(getdate())");
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.UserId).HasName("PK__User__1788CCACB96A9892");

            entity.HasIndex(e => e.Email, "IX_User_Email_NotNull")
                .IsUnique()
                .HasFilter("([Email] IS NOT NULL)");

            entity.HasIndex(e => e.Phone, "IX_User_Phone_NotNull")
                .IsUnique()
                .HasFilter("([Phone] IS NOT NULL)");

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(getdate())");
            entity.Property(e => e.IsDeleted).HasDefaultValue(false);
            entity.Property(e => e.UpdatedAt).HasDefaultValueSql("(getdate())");
            entity.Property(e => e.Verified).HasDefaultValue(false);

            entity.HasOne(d => d.UserRole).WithMany(p => p.Users)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__User__UserRoleID__3D5E1FD2");
        });

        modelBuilder.Entity<UserRole>(entity =>
        {
            entity.HasKey(e => e.UserRoleId).HasName("PK__UserRole__3D978A5519653ECB");

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(getdate())");
            entity.Property(e => e.IsDeleted).HasDefaultValue(false);
            entity.Property(e => e.UpdatedAt).HasDefaultValueSql("(getdate())");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
