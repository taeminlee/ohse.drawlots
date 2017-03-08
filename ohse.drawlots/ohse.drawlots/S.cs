using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ohse.drawlots
{
    internal static class S
    {
        internal static DrawlotsModel DB = new DrawlotsModel(WorkingConn.GetConnStr());

        internal static void Migration()
        {
            if (System.IO.File.Exists(DBConn.GetPath()))
            {
                System.IO.File.Move(DBConn.GetPath(), DBConn.GetMigrationPath());

                databaseEntities oldDB = new databaseEntities(DBConn.GetConnStr(DBConn.GetMigrationPath()));

                S.DB.@class.AddRange(oldDB.@class.Select(c => new class2()
                {
                    cid = c.cid,
                    class1 = c.class1,
                    year = c.year
                }));
                S.DB.forget.AddRange(oldDB.@forget.Select(forget => new forget2()
                {
                    cid = forget.cid,
                    datetime = forget.datetime
                }));
                S.DB.group.AddRange(oldDB.group.Select(g => new group2()
                {
                    cid = g.cid,
                    gid = g.gid,
                    name = g.name
                }));
                S.DB.history.AddRange(oldDB.history.Select(history => new history2()
                {
                    cid = history.cid,
                    sid = history.sid,
                    date = history.date
                }));
                S.DB.student.AddRange(oldDB.student.Select(student => new student2()
                {
                    cid = student.cid,
                    gid = student.gid,
                    sid = student.sid,
                    name = student.name,
                    num = student.num
                }));

                oldDB.Dispose();
                S.DB.SaveChanges();
            } else if (System.IO.File.Exists(DBConn.GetMigrationPath()))
            {
                System.IO.File.Delete(DBConn.GetMigrationPath());
            }
        }
    }
}
