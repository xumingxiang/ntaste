namespace org.apache.mahout.cf.taste.impl.common
{
    using org.apache.mahout.cf.taste;
    using org.apache.mahout.cf.taste.common;
    using System;
    using System.Collections.Generic;
    using System.Threading;

    public sealed class RefreshHelper : Refreshable
    {
        private List<Refreshable> dependencies = new List<Refreshable>(3);
        private static Logger log = LoggerFactory.getLogger(typeof(RefreshHelper));
        private Action refreshRunnable;

        public RefreshHelper(Action refreshRunnable)
        {
            this.refreshRunnable = refreshRunnable;
        }

        public void addDependency(Refreshable refreshable)
        {
            if (refreshable != null)
            {
                this.dependencies.Add(refreshable);
            }
        }

        public static IList<Refreshable> buildRefreshed(IList<Refreshable> currentAlreadyRefreshed)
        {
            return ((currentAlreadyRefreshed == null) ? new List<Refreshable>(3) : currentAlreadyRefreshed);
        }

        public static void maybeRefresh(IList<Refreshable> alreadyRefreshed, Refreshable refreshable)
        {
            if (!alreadyRefreshed.Contains(refreshable))
            {
                alreadyRefreshed.Add(refreshable);
                log.info("Added refreshable: {}", new object[] { refreshable });
                refreshable.refresh(alreadyRefreshed);
                log.info("Refreshed: {}", new object[] { alreadyRefreshed });
            }
        }

        public void refresh(IList<Refreshable> alreadyRefreshed)
        {
            if (Monitor.TryEnter(this))
            {
                try
                {
                    alreadyRefreshed = buildRefreshed(alreadyRefreshed);
                    foreach (Refreshable refreshable in this.dependencies)
                    {
                        maybeRefresh(alreadyRefreshed, refreshable);
                    }
                    if (this.refreshRunnable != null)
                    {
                        try
                        {
                            this.refreshRunnable();
                        }
                        catch (Exception exception)
                        {
                            log.warn("Unexpected exception while refreshing", new object[] { exception });
                        }
                    }
                }
                finally
                {
                    Monitor.Exit(this);
                }
            }
        }

        public void removeDependency(Refreshable refreshable)
        {
            if (refreshable != null)
            {
                this.dependencies.Remove(refreshable);
            }
        }
    }
}