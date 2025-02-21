using A1RConsole.DB;
using A1RConsole.Models.Production.Grading;
using A1RConsole.Models.Production.Mixing;
using A1RConsole.Models.Production.Peeling;
using A1RConsole.Models.Production.ReRolling;
using A1RConsole.Models.Production.Slitting;
using A1RConsole.Models.Stock;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace A1RConsole.Core
{
    public static class ProductionManager
    {


        //GRADING
        public static int AddToGrading(List<GradingOrder> gradingOrder)
        {
            GradingManager gm = new GradingManager();
            return gm.AddToGrading(gradingOrder);
        }

        //MIXING
        public static int AddToMixing(List<MixingOrder> mixingOrders)
        {
            MixingManager mm = new MixingManager();
            return mm.ProcessMixingOrder(mixingOrders);
        }

        //SLITTING
        public static int AddToSlitting(List<SlittingOrder> slittingOrder)
        {
            //SlittingManager sm = new SlittingManager();
            //return sm.ProcessSlittingOrder(slittingOrder);
            return 0;

        }
        //PEELING
        public static int AddToPeeling(List<PeelingOrder> peelingOrder)
        {
            PeelingManager pm = new PeelingManager();
            return pm.ProcessPeelingOrder(peelingOrder);

        }

        //RE-ROLLING
        public static int AddToReRolling(ReRollingOrder reRollingOrder)
        {
            ReRollingManager rrm = new ReRollingManager();
            return rrm.ProcessReRollingOrder(reRollingOrder);
        }

        //GRADED STOCK
        public static int AddToGradedStock(List<GradedStock> gradedStock)
        {
            return DBAccess.InsertGradedStock(gradedStock, 0, DateTime.Now, false);
        }

    }
}

