namespace MapAccounts.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class InitialCreate : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.GSPanoramas",
                c => new
                    {
                        ID = c.Int(nullable: false, identity: true),
                        panoID = c.String(),
                        frontAngle = c.Double(nullable: false),
                        pitch = c.Double(nullable: false),
                    })
                .PrimaryKey(t => t.ID);
            
            CreateTable(
                "dbo.GSPictures",
                c => new
                    {
                        ID = c.Int(nullable: false, identity: true),
                        GSPanoramaID = c.Int(nullable: false),
                        heading = c.Double(nullable: false),
                        imageURI = c.String(),
                    })
                .PrimaryKey(t => t.ID)
                .ForeignKey("dbo.GSPanoramas", t => t.GSPanoramaID, cascadeDelete: true)
                .Index(t => t.GSPanoramaID);
            
            CreateTable(
                "dbo.RegionModels",
                c => new
                    {
                        ID = c.Int(nullable: false, identity: true),
                        ApplicationUserID = c.String(maxLength: 128),
                        Bounds_South = c.Double(nullable: false),
                        Bounds_West = c.Double(nullable: false),
                        Bounds_North = c.Double(nullable: false),
                        Bounds_East = c.Double(nullable: false),
                    })
                .PrimaryKey(t => t.ID)
                .ForeignKey("dbo.AspNetUsers", t => t.ApplicationUserID)
                .Index(t => t.ApplicationUserID);
            
            CreateTable(
                "dbo.AspNetUsers",
                c => new
                    {
                        Id = c.String(nullable: false, maxLength: 128),
                        Email = c.String(maxLength: 256),
                        EmailConfirmed = c.Boolean(nullable: false),
                        PasswordHash = c.String(),
                        SecurityStamp = c.String(),
                        PhoneNumber = c.String(),
                        PhoneNumberConfirmed = c.Boolean(nullable: false),
                        TwoFactorEnabled = c.Boolean(nullable: false),
                        LockoutEndDateUtc = c.DateTime(),
                        LockoutEnabled = c.Boolean(nullable: false),
                        AccessFailedCount = c.Int(nullable: false),
                        UserName = c.String(nullable: false, maxLength: 256),
                    })
                .PrimaryKey(t => t.Id)
                .Index(t => t.UserName, unique: true, name: "UserNameIndex");
            
            CreateTable(
                "dbo.AspNetUserClaims",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        UserId = c.String(nullable: false, maxLength: 128),
                        ClaimType = c.String(),
                        ClaimValue = c.String(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.AspNetUsers", t => t.UserId, cascadeDelete: true)
                .Index(t => t.UserId);
            
            CreateTable(
                "dbo.AspNetUserLogins",
                c => new
                    {
                        LoginProvider = c.String(nullable: false, maxLength: 128),
                        ProviderKey = c.String(nullable: false, maxLength: 128),
                        UserId = c.String(nullable: false, maxLength: 128),
                    })
                .PrimaryKey(t => new { t.LoginProvider, t.ProviderKey, t.UserId })
                .ForeignKey("dbo.AspNetUsers", t => t.UserId, cascadeDelete: true)
                .Index(t => t.UserId);
            
            CreateTable(
                "dbo.AspNetUserRoles",
                c => new
                    {
                        UserId = c.String(nullable: false, maxLength: 128),
                        RoleId = c.String(nullable: false, maxLength: 128),
                    })
                .PrimaryKey(t => new { t.UserId, t.RoleId })
                .ForeignKey("dbo.AspNetUsers", t => t.UserId, cascadeDelete: true)
                .ForeignKey("dbo.AspNetRoles", t => t.RoleId, cascadeDelete: true)
                .Index(t => t.UserId)
                .Index(t => t.RoleId);
            
            CreateTable(
                "dbo.StreetModels",
                c => new
                    {
                        ID = c.Int(nullable: false, identity: true),
                        Name = c.String(),
                    })
                .PrimaryKey(t => t.ID);
            
            CreateTable(
                "dbo.StreetPointModels",
                c => new
                    {
                        ID = c.Int(nullable: false, identity: true),
                        lat = c.Double(nullable: false),
                        lng = c.Double(nullable: false),
                        StreetModelID = c.Int(nullable: false),
                        GSPanoramaID = c.Int(),
                    })
                .PrimaryKey(t => t.ID)
                .ForeignKey("dbo.GSPanoramas", t => t.GSPanoramaID)
                .ForeignKey("dbo.StreetModels", t => t.StreetModelID, cascadeDelete: true)
                .Index(t => t.StreetModelID)
                .Index(t => t.GSPanoramaID);
            
            CreateTable(
                "dbo.AspNetRoles",
                c => new
                    {
                        Id = c.String(nullable: false, maxLength: 128),
                        Name = c.String(nullable: false, maxLength: 256),
                    })
                .PrimaryKey(t => t.Id)
                .Index(t => t.Name, unique: true, name: "RoleNameIndex");
            
            CreateTable(
                "dbo.StreetModelRegionModels",
                c => new
                    {
                        StreetModel_ID = c.Int(nullable: false),
                        RegionModel_ID = c.Int(nullable: false),
                    })
                .PrimaryKey(t => new { t.StreetModel_ID, t.RegionModel_ID })
                .ForeignKey("dbo.StreetModels", t => t.StreetModel_ID, cascadeDelete: true)
                .ForeignKey("dbo.RegionModels", t => t.RegionModel_ID, cascadeDelete: true)
                .Index(t => t.StreetModel_ID)
                .Index(t => t.RegionModel_ID);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.AspNetUserRoles", "RoleId", "dbo.AspNetRoles");
            DropForeignKey("dbo.StreetPointModels", "StreetModelID", "dbo.StreetModels");
            DropForeignKey("dbo.StreetPointModels", "GSPanoramaID", "dbo.GSPanoramas");
            DropForeignKey("dbo.StreetModelRegionModels", "RegionModel_ID", "dbo.RegionModels");
            DropForeignKey("dbo.StreetModelRegionModels", "StreetModel_ID", "dbo.StreetModels");
            DropForeignKey("dbo.AspNetUserRoles", "UserId", "dbo.AspNetUsers");
            DropForeignKey("dbo.RegionModels", "ApplicationUserID", "dbo.AspNetUsers");
            DropForeignKey("dbo.AspNetUserLogins", "UserId", "dbo.AspNetUsers");
            DropForeignKey("dbo.AspNetUserClaims", "UserId", "dbo.AspNetUsers");
            DropForeignKey("dbo.GSPictures", "GSPanoramaID", "dbo.GSPanoramas");
            DropIndex("dbo.StreetModelRegionModels", new[] { "RegionModel_ID" });
            DropIndex("dbo.StreetModelRegionModels", new[] { "StreetModel_ID" });
            DropIndex("dbo.AspNetRoles", "RoleNameIndex");
            DropIndex("dbo.StreetPointModels", new[] { "GSPanoramaID" });
            DropIndex("dbo.StreetPointModels", new[] { "StreetModelID" });
            DropIndex("dbo.AspNetUserRoles", new[] { "RoleId" });
            DropIndex("dbo.AspNetUserRoles", new[] { "UserId" });
            DropIndex("dbo.AspNetUserLogins", new[] { "UserId" });
            DropIndex("dbo.AspNetUserClaims", new[] { "UserId" });
            DropIndex("dbo.AspNetUsers", "UserNameIndex");
            DropIndex("dbo.RegionModels", new[] { "ApplicationUserID" });
            DropIndex("dbo.GSPictures", new[] { "GSPanoramaID" });
            DropTable("dbo.StreetModelRegionModels");
            DropTable("dbo.AspNetRoles");
            DropTable("dbo.StreetPointModels");
            DropTable("dbo.StreetModels");
            DropTable("dbo.AspNetUserRoles");
            DropTable("dbo.AspNetUserLogins");
            DropTable("dbo.AspNetUserClaims");
            DropTable("dbo.AspNetUsers");
            DropTable("dbo.RegionModels");
            DropTable("dbo.GSPictures");
            DropTable("dbo.GSPanoramas");
        }
    }
}
