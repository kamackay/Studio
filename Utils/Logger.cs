using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Electrum {
    public class Logger {
        private Logger instance = null;

        public Logger getLogger() {
            if (instance != null) return instance;
            else {
                instance = new Logger();
                return instance;
            }
        }

        private Logger() {

        }

        private ActionQueue queue;

        public class ActionQueue {

            public ActionQueue() {

            }
        }
    }
}
