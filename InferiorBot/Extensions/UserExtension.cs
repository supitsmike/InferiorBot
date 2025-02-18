using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Infrastructure.InferiorBot;

namespace InferiorBot.Extensions
{
    public static class UserExtension
    {
        public static void WriteAuditLog(this InferiorBotContext context, decimal userId, string tableName, string columnName)
        {
            context.AuditLogs.AddAsync(new AuditLog
            {
                LogId = Guid.NewGuid(),
                UserId = userId,
                TableName = "",
                ColumnName = "",
                NewData = "",
                PreviousData = ""
            });
        }
    }
}
