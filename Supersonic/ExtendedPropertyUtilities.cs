// Copyright (c) 2009-2015 Pwnt & Co. All Right Reserved.
// 
// Filename:  ExtendedPropertyUtilities.cs
// Author:    Stephen C. Austin (stephen.austin)
// Modified:  03/04/2015 11:06 PM

using System.Data.SqlClient;

namespace Supersonic
{
    /// <summary>
    /// </summary>
    public static class ExtendedPropertyUtilities
    {
        #region SQL Commands

        private const string REMOVE_EP = @" DECLARE @SchemaName nvarchar(120) = N'{0}'
                                            DECLARE @TableName nvarchar(120) = N'{1}'
                                            DECLARE @EPName nvarchar(120) = N'{2}'

                                            BEGIN
                                            IF ((SELECT COUNT(*) FROM sys.fn_listextendedproperty(NULL, 'SCHEMA', @SchemaName, '{3}', @TableName, NULL, NULL) WHERE name = @EPName) = 1)
                                                EXEC sys.sp_dropextendedproperty @name=@EPName, @level0type=N'SCHEMA',@level0name=@SchemaName, @level1type='{3}',@level1name=@TableName 
                                            END";

        private const string SET_EP = @"DECLARE @SchemaName nvarchar(120) = N'{0}'
                                        DECLARE @TableName nvarchar(120) = N'{1}'
                                        DECLARE @EPName nvarchar(120) = N'{2}'
                                        DECLARE @EPValue nvarchar(120) = N'{3}'

                                        BEGIN
                                        IF ((SELECT COUNT(*) FROM sys.fn_listextendedproperty(NULL, 'SCHEMA', @SchemaName, '{4}', @TableName, NULL, NULL) WHERE name = @EPName) = 1)
                                            EXEC sys.sp_updateextendedproperty @name=@EPName, @value=@EPValue , @level0type=N'SCHEMA',@level0name=@SchemaName, @level1type='{4}',@level1name=@TableName 
                                        ELSE 
                                            EXEC sys.sp_addextendedproperty @name=@EPName, @value=@EPValue , @level0type=N'SCHEMA',@level0name=@SchemaName, @level1type='{4}',@level1name=@TableName
                                        END";

        #endregion

        #region Tables

        /// <summary>
        /// Add the enum generation hints to the table (self) so that the framework creates an enum instead of a class to represent
        /// the table.
        /// </summary>
        /// <param name="self"></param>
        /// <param name="connectionString"></param>
        /// <param name="enumNameColumn"></param>
        /// <param name="enumValueColumn"></param>
        public static void SetEnum(this Table self, string connectionString, string enumNameColumn, string enumValueColumn)
        {
            Execute(connectionString,
                    string.Format(SET_EP, self.Schema, self.SqlName, "EF_EnumName", enumNameColumn, self.IsTable ? "TABLE" : self.IsStoredProcedure ? "PROCEDURE" : "VIEW"),
                    string.Format(SET_EP, self.Schema, self.SqlName, "EF_EnumValue", enumValueColumn, self.IsTable ? "TABLE" : self.IsStoredProcedure ? "PROCEDURE" : "VIEW"));
            self.EnumNameColumn = enumNameColumn;
            self.EnumValueColumn = enumValueColumn;
            var reader = new SqlServerSchemaReader(connectionString);
            reader.FillEnum(self);
        }

        /// <summary>
        /// Removes the enum generation hints from the table.
        /// </summary>
        /// <param name="self"></param>
        /// <param name="connectionString"></param>
        public static void ClearEnum(this Table self, string connectionString)
        {
            Execute(connectionString,
                    string.Format(REMOVE_EP, self.Schema, self.SqlName, "EF_EnumName", self.IsTable ? "TABLE" : self.IsStoredProcedure ? "PROCEDURE" : "VIEW"),
                    string.Format(REMOVE_EP, self.Schema, self.SqlName, "EF_EnumValue", self.IsTable ? "TABLE" : self.IsStoredProcedure ? "PROCEDURE" : "VIEW"));
            self.EnumNameColumn = null;
            self.EnumValueColumn = null;
            self.EnumMembers.Clear();
        }

        /// <summary>
        /// Hint to include the entity
        /// </summary>
        /// <param name="self"></param>
        /// <param name="connectionString"></param>
        public static void Include(this Table self, string connectionString)
        {
            Execute(connectionString, self.IsView
                                          ? string.Format(SET_EP, self.Schema, self.SqlName, "EF_Include", string.Empty, self.IsTable ? "TABLE" : self.IsStoredProcedure ? "PROCEDURE" : "VIEW")
                                          : string.Format(REMOVE_EP, self.Schema, self.SqlName, "EF_Omit", self.IsTable ? "TABLE" : self.IsStoredProcedure ? "PROCEDURE" : "VIEW"));
            self.IsOmitted = false;
        }

        /// <summary>
        /// Hint to exclude the entity
        /// </summary>
        /// <param name="self"></param>
        /// <param name="connectionString"></param>
        public static void Exclude(this Table self, string connectionString)
        {
            Execute(connectionString, self.IsView
                                          ? string.Format(REMOVE_EP, self.Schema, self.SqlName, "EF_Include", self.IsTable ? "TABLE" : self.IsStoredProcedure ? "PROCEDURE" : "VIEW")
                                          : string.Format(SET_EP, self.Schema, self.SqlName, "EF_Omit", string.Empty, self.IsTable ? "TABLE" : self.IsStoredProcedure ? "PROCEDURE" : "VIEW"));
            self.IsOmitted = true;
        }

        #endregion

        #region Stored Procedures

        /// <summary>
        /// </summary>
        /// <param name="self"></param>
        /// <param name="connectionString"></param>
        /// <param name="methodName"></param>
        /// <param name="containerName"></param>
        /// <param name="executionMode"></param>
        public static void Include(this StoredProcedure self, string connectionString, string methodName = null, string containerName = "StoredProcedures", string executionMode = "VoidResult")
        {
            Execute(connectionString,
                    string.Format(SET_EP, self.Schema, self.SqlName, "EF_Include", string.Empty, "PROCEDURE"),
                    string.Format(SET_EP, self.Schema, self.SqlName, "EF_ExecutionMode", string.IsNullOrEmpty(executionMode) ? "VoidResult" : executionMode, "PROCEDURE"),
                    string.Format(SET_EP, self.Schema, self.SqlName, "EF_MethodName", string.IsNullOrEmpty(methodName) ? self.CodeName : methodName, "PROCEDURE"),
                    string.Format(SET_EP, self.Schema, self.SqlName, "EF_ContainerName", string.IsNullOrEmpty(containerName) ? "StoredProcedures" : containerName, "PROCEDURE"));
            self.IsOmitted = false;
        }

        /// <summary>
        /// </summary>
        /// <param name="self"></param>
        /// <param name="connectionString"></param>
        public static void Exclude(this StoredProcedure self, string connectionString)
        {
            Execute(connectionString,
                    string.Format(REMOVE_EP, self.Schema, self.SqlName, "EF_Include", "PROCEDURE"),
                    string.Format(REMOVE_EP, self.Schema, self.SqlName, "EF_ExecutionMode", "PROCEDURE"),
                    string.Format(REMOVE_EP, self.Schema, self.SqlName, "EF_MethodName", "PROCEDURE"),
                    string.Format(REMOVE_EP, self.Schema, self.SqlName, "EF_ContainerName", "PROCEDURE"));
            self.IsOmitted = true;
        }

        #endregion

        private static void Execute(string connectionString, params string[] commandTexts)
        {
            var command = new SqlCommand(string.Empty, new SqlConnection(connectionString));
            using (command)
            {
                command.Connection.Open();
                foreach (var commandText in commandTexts)
                {
                    command.CommandText = commandText;
                    command.ExecuteNonQuery();
                }
                command.Connection.Close();
            }
        }
    }
}