using A1RConsole.DB;
using A1RConsole.Models.Capacity;
using A1RConsole.Models.Machines;
using A1RConsole.Models.Production;
using A1RConsole.Models.Production.Grading;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace A1RConsole.Core
{
    public class Production
    {
        private List<CapacityInfo> capacityInfoList;

        public List<ProductionTimeTable> NewDateList { get; set; }
        private List<Machine> machinesList;
        private List<GradingDefaultCapacity> gradingDefaultCapacityList;
        private List<MixingDefaultCapacity> mixingDefaultCapacityList;
        private List<SlittingDefaultCapacity> slittingDefaultCapacityList;
        private List<PeelingDefaultCapacity> peelingDefaultCapacityList;
        private List<ReRollingDefaultCapacity> reRollingDefaultCapacityList;

        public Production()
        {
            machinesList = DBAccess.GetProductionMachines();
            gradingDefaultCapacityList = DBAccess.GetAllGradingDefaultCapacities();
            mixingDefaultCapacityList = DBAccess.GetAllMixingDefaultCapacities();
            slittingDefaultCapacityList = DBAccess.GetAllSlittingDefaultCapacities();
            peelingDefaultCapacityList = DBAccess.GetAllPeelingDefaultCapacities();
            reRollingDefaultCapacityList = DBAccess.GetAllReRollingDefaultCapacities();
        }

        public bool AddNewDates(DateTime startDate, bool enableShifts)
        {
            bool datesAdded = false;
            BusinessDaysGenerator bds = new BusinessDaysGenerator();
            capacityInfoList = new List<CapacityInfo>();

            if (gradingDefaultCapacityList.Count > 0 && mixingDefaultCapacityList.Count > 0 && slittingDefaultCapacityList.Count > 0)
            {
                for (int i = 0; i < 7; i++)
                {
                    int r = DBAccess.CheckDateAvailable(startDate);//Date not found
                    if (r == 0)
                    {

                        foreach (var itemML in machinesList)
                        {
                            CapacityInfo ci = new CapacityInfo();
                            List<GradingDefaultCapacity> tempGDCL = new List<GradingDefaultCapacity>();
                            List<MixingDefaultCapacity> tempMDC = new List<MixingDefaultCapacity>();
                            List<SlittingDefaultCapacity> tempSDCL = new List<SlittingDefaultCapacity>();
                            List<PeelingDefaultCapacity> tempPDCL = new List<PeelingDefaultCapacity>();
                            List<ReRollingDefaultCapacity> tempRRDCL = new List<ReRollingDefaultCapacity>();

                            foreach (var itemGDCL in gradingDefaultCapacityList)
                            {
                                if (itemGDCL.Day == startDate.DayOfWeek.ToString() && (itemGDCL.Machine.MachineID == 1 || itemGDCL.Machine.MachineID == 7))
                                {
                                    GradingDefaultCapacity gdcl = new GradingDefaultCapacity();
                                    gdcl.ID = itemGDCL.ID;
                                    gdcl.Machine = new Machine() { MachineID = itemGDCL.Machine.MachineID };
                                    gdcl.RubberGrade = new RubberGrades() { GradeID = itemGDCL.RubberGrade.GradeID };
                                    gdcl.Capacity = itemGDCL.Capacity;
                                    gdcl.Shift = itemGDCL.Shift;
                                    gdcl.Day = itemGDCL.Day;
                                    tempGDCL.Add(gdcl);
                                }
                            }

                            foreach (var itemMDCL in mixingDefaultCapacityList)
                            {
                                if (itemMDCL.Day == startDate.DayOfWeek.ToString() && (itemMDCL.Machine.MachineID == 2 || itemMDCL.Machine.MachineID == 3))
                                {
                                    MixingDefaultCapacity mdcl = new MixingDefaultCapacity();
                                    mdcl.ID = itemMDCL.ID;
                                    mdcl.Machine = new Machine() { MachineID = itemMDCL.Machine.MachineID };
                                    mdcl.MaxMixes = itemMDCL.MaxMixes;
                                    mdcl.Day = itemMDCL.Day;
                                    tempMDC.Add(mdcl);
                                }
                            }
                            foreach (var itemSDCL in slittingDefaultCapacityList)
                            {
                                if (itemSDCL.Day == startDate.DayOfWeek.ToString() && (itemSDCL.Machine.MachineID == 4 || itemSDCL.Machine.MachineID == 8))
                                {
                                    SlittingDefaultCapacity sldc = new SlittingDefaultCapacity();
                                    sldc.ID = itemSDCL.ID;
                                    sldc.Machine = new Machine() { MachineID = itemSDCL.Machine.MachineID };
                                    sldc.DollarValue = itemSDCL.DollarValue;
                                    sldc.Shift = itemSDCL.Shift;
                                    sldc.Day = itemSDCL.Day;
                                    tempSDCL.Add(sldc);
                                }
                            }

                            foreach (var itemPDCL in peelingDefaultCapacityList)
                            {
                                if (itemPDCL.Day == startDate.DayOfWeek.ToString() && (itemPDCL.Machine.MachineID == 5))
                                {
                                    PeelingDefaultCapacity pdc = new PeelingDefaultCapacity();
                                    pdc.ID = itemPDCL.ID;
                                    pdc.Machine = new Machine() { MachineID = itemPDCL.Machine.MachineID };
                                    pdc.DollarValue = itemPDCL.DollarValue;
                                    pdc.Shift = itemPDCL.Shift;
                                    pdc.Day = itemPDCL.Day;
                                    tempPDCL.Add(pdc);
                                }
                            }

                            foreach (var itemRRDCL in reRollingDefaultCapacityList)
                            {
                                if (itemRRDCL.Day == startDate.DayOfWeek.ToString() && (itemRRDCL.Machine.MachineID == 6))
                                {
                                    ReRollingDefaultCapacity rrdc = new ReRollingDefaultCapacity();
                                    rrdc.ID = itemRRDCL.ID;
                                    rrdc.Machine = new Machine() { MachineID = itemRRDCL.Machine.MachineID };
                                    rrdc.DollarValue = itemRRDCL.DollarValue;
                                    rrdc.Shift = itemRRDCL.Shift;
                                    rrdc.Day = itemRRDCL.Day;
                                    tempRRDCL.Add(rrdc);
                                }
                            }

                            ci.ProductionTimeTable = new ProductionTimeTable();
                            ci.ProductionTimeTable.MachineID = itemML.MachineID;
                            ci.ProductionTimeTable.ProductionDate = startDate;
                            ci.ProductionTimeTable.IsMachineActive = true;
                            ci.ProductionTimeTable.IsDayShiftActive = true;
                            ci.ProductionTimeTable.IsEveningShiftActive = true;
                            ci.ProductionTimeTable.IsNightShiftActive = true;
                            ci.GradingCapacityList = tempGDCL;
                            ci.MixingCapacityList = tempMDC;
                            ci.SlittingCapacityList = tempSDCL;
                            ci.PeelingCapacityList = tempPDCL;
                            ci.ReRollingCapacityList = tempRRDCL;

                            capacityInfoList.Add(ci);
                        }
                    }
                    startDate = bds.AddBusinessDays(startDate, 1);
                }

                if (capacityInfoList.Count > 0)
                {
                    int res2 = DBAccess.InsertNewPrdDates(capacityInfoList, machinesList);
                    if (res2 != 0)
                    {
                        datesAdded = true;
                        Console.WriteLine("Date list created and sent to database");
                    }
                }
            }
            return datesAdded;
        }
    }
}
