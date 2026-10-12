using DAL.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DAL.Configurations
{
    public class ReturnAttachmentConfiguration : IEntityTypeConfiguration<ReturnAttachment>
    {
        public void Configure(EntityTypeBuilder<ReturnAttachment> builder)
        {
            builder.ToTable("ReturnAttachments");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.FilePath)
                .IsRequired()
                .HasMaxLength(500);

            builder.Property(x => x.FileName)
                .IsRequired()
                .HasMaxLength(255);

            builder.HasOne(x => x.ReturnRequest)
                .WithMany(x => x.Attachments)
                .HasForeignKey(x => x.ReturnRequestId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
