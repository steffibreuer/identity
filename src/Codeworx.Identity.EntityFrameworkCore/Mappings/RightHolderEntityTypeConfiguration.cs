﻿using Codeworx.Identity.EntityFrameworkCore.Model;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Codeworx.Identity.EntityFrameworkCore.Mappings
{
    public class RightHolderEntityTypeConfiguration : IEntityTypeConfiguration<RightHolder>
    {
        public void Configure(EntityTypeBuilder<RightHolder> builder)
        {
            builder.ToTable("RightHolder");

            builder.HasDiscriminator<string>("Type")
                   .HasValue<User>("User")
                   .HasValue<Role>("Role");
        }
    }
}