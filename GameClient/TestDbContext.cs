using GameData;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameClient
{
        class TestDbContext : IDisposable
        {
            private bool disposed = false;
            public List<PlayerData> ScoreBoard = new List<PlayerData>();

            public TestDbContext()
            {
                //ScoreBoard = File.ReadAllLines(@"Content\random Names with scores.csv")
                //                             //.Skip(1) // Only needed if the first row contains the Field names
                //                             .Select(v => PlayerData.FromCsv(v))
                //                             .OrderByDescending(s => s.topscore)
                //                             .ToList();
            }

            public void Dispose()
            {
                Dispose(true);
                GC.SuppressFinalize(this);
            }

            protected virtual void Dispose(bool disposing)
            {

                if (!disposed)
                {
                    if (disposing)
                    {
                        // Manual release of managed resources.
                    }
                    // Release unmanaged resources.
                    disposed = true;
                }
            }

            public List<PlayerData> getTop(int count)
            {
                return ScoreBoard.Take(count).ToList();
            }
        }
}
