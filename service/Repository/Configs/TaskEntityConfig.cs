using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using service.Models;

namespace service.Repository.Configs
{
    public class TaskEntityConfig : IEntityTypeConfiguration<ScheduleTask>
    {
        public void Configure(EntityTypeBuilder<ScheduleTask> builder)
        {
            builder
                .Property(p => p.Id)
                .ValueGeneratedOnAdd()
                .HasColumnOrder(0)
                .HasColumnName("Id")
                .HasColumnType("int")
                .HasComment("Chave para identificar a tarefa");
            builder
                .Property(p => p.TaskName)
                .HasColumnOrder(1)
                .HasColumnName("TaskName")
                .HasColumnType("varchar(50)")
                .HasComment("Nome para identificar nome tarefa")
                .IsRequired();
            builder
                .Property(p => p.Action)
                .HasColumnOrder(2)
                .HasColumnName("Action")
                .HasColumnType("varchar(10)")
                .HasComment("Ação para identificar comportamento da placa")
                .IsRequired();
            builder
                .Property(p => p.Expression)
                .HasColumnOrder(3)
                .HasColumnName("Expression")
                .HasColumnType("varchar(30)") //00 20 * * 5
                .HasComment("Expressão cron para a programação")
                .IsRequired();
            builder
                .Property(p => p.TaskJobId)
                .HasColumnOrder(4)
                .HasColumnName("TaskJobId")
                .HasColumnType("varchar(80)")
                .HasComment("Identificação da task cadastrada")
                .IsRequired();
            builder
                .Property(p => p.EquipamentId)
                .HasColumnOrder(5)
                .HasColumnName("EquipamentId")
                .HasColumnType("uniqueidentifier")
                .HasComment("Identificação da task cadastrada")
                .IsRequired();

            builder
                .HasOne(p => p.Equipament)
                .WithMany(p => p.Tasks)
                .HasForeignKey(p => p.EquipamentId)
                .HasConstraintName("fk_equip_task")
                .IsRequired();

            builder
                .HasIndex(p => new
                { p.Action, p.Expression, p.EquipamentId })
                .HasDatabaseName("idx_task_unique")
                .IsUnique();

            builder
                .ToTable("Task", "domain")
                .HasKey(c => c.Id)
                .HasName("pk_task");
        }
    }
}

