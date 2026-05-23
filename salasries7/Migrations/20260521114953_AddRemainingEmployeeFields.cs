using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace salasries7.Migrations
{
    /// <inheritdoc />
    public partial class AddRemainingEmployeeFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            var sql = @"
                IF COL_LENGTH('Employees', 'NationalId') IS NULL
                    ALTER TABLE Employees ADD NationalId nvarchar(30) NULL;

                IF COL_LENGTH('Employees', 'PassportNumber') IS NULL
                    ALTER TABLE Employees ADD PassportNumber nvarchar(50) NULL;

                IF COL_LENGTH('Employees', 'PassportExpiry') IS NULL
                    ALTER TABLE Employees ADD PassportExpiry datetime2 NULL;

                IF COL_LENGTH('Employees', 'JobTitle') IS NULL
                    ALTER TABLE Employees ADD JobTitle nvarchar(100) NULL;

                IF COL_LENGTH('Employees', 'Department') IS NULL
                    ALTER TABLE Employees ADD Department nvarchar(100) NULL;

                IF COL_LENGTH('Employees', 'PhoneNumber') IS NULL
                    ALTER TABLE Employees ADD PhoneNumber nvarchar(20) NULL;

                IF COL_LENGTH('Employees', 'PersonalEmail') IS NULL
                    ALTER TABLE Employees ADD PersonalEmail nvarchar(100) NULL;

                IF COL_LENGTH('Employees', 'Notes') IS NULL
                    ALTER TABLE Employees ADD Notes nvarchar(500) NULL;
            ";

            migrationBuilder.Sql(sql);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
