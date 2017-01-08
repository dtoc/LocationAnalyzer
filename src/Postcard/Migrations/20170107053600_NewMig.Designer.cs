using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Postcard.Migrations
{
    [DbContext(typeof(PostcardContext))]
    [Migration("20170107053600_NewMig")]
    partial class NewMig
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
            modelBuilder
                .HasAnnotation("ProductVersion", "1.1.0-rtm-22752")
                .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

            modelBuilder.Entity("PlaceNode", b =>
                {
                    b.Property<int>("PlaceNodeID")
                        .ValueGeneratedOnAdd();

                    b.Property<int>("NumberOfStatesThatHaveThisPlace");

                    b.Property<string>("PlaceName");

                    b.Property<string>("StateName");

                    b.Property<string>("link");

                    b.HasKey("PlaceNodeID");

                    b.ToTable("PlaceNodes");
                });
        }
    }
}
