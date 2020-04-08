using System;
using System.IO;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;

namespace DmvAppointmentScheduler
{
    class Program
    {
        public static Random random = new Random();
        public static List<Appointment> appointmentList = new List<Appointment>();
        static void Main(string[] args)
        {
            CustomerList customers = ReadCustomerData();
            TellerList tellers = ReadTellerData();
            Calculation(customers, tellers);
            OutputTotalLengthToConsole();

        }
        private static CustomerList ReadCustomerData()
        {
            string fileName = "CustomerData.json";
            string path = Path.Combine(Environment.CurrentDirectory, @"InputData\", fileName);
            string jsonString = File.ReadAllText(path);
            CustomerList customerData = JsonConvert.DeserializeObject<CustomerList>(jsonString);
            return customerData;

        }
        private static TellerList ReadTellerData()
        {
            string fileName = "TellerData.json";
            string path = Path.Combine(Environment.CurrentDirectory, @"InputData\", fileName);
            string jsonString = File.ReadAllText(path);
            TellerList tellerData = JsonConvert.DeserializeObject<TellerList>(jsonString);
            return tellerData;

        }
        static void Calculation(CustomerList customers, TellerList tellers)
        {
            int count = 0;

            double total = customers.Customer.Sum(item => Convert.ToInt32(item.duration));
            double average = total / tellers.Teller.Count;

            //When the dmv opens each teller is assigned with a customer
            bool isFirstLoop = true;

 
            foreach (Customer customer in customers.Customer)
            {
                if (count == 149)
                {
                    isFirstLoop = false;
                }

                if (!isFirstLoop)
                {
                    //The customer will be sent to the next available Teller with less duration
                    //So that the teller is not waiting for the other teller till all the customers are done.
                    IEnumerable<TellerAppointment> tellerAppointments = TellerTotalDuration();
                    var minDurationTellerDuration = tellerAppointments.OrderBy(mt => mt.totalDuration);
                    var minDurationTeller = minDurationTellerDuration.FirstOrDefault().teller.id;
                    count = tellers.Teller.FindIndex(t => t.id == minDurationTeller);
   
                }

                var appointment = new Appointment(customer, tellers.Teller[count]);
                appointmentList.Add(appointment);

                //First 150 customers will be assigned to 150 Tellers
                if (isFirstLoop)
                {
                    count++;
                }

            }
        }
        static void OutputTotalLengthToConsole()
        {
            IEnumerable<TellerAppointment> tellerAppointments = TellerTotalDuration();

            var max = tellerAppointments.OrderBy(i => i.totalDuration);
            foreach (var m in max)
                Console.WriteLine("Teller " + m.teller.id + " will work for " + m.totalDuration + " minutes!");
        }

        public static IEnumerable<TellerAppointment> TellerTotalDuration()
        {
            IEnumerable<TellerAppointment> tellerDuration =
                from appointment in appointmentList
                group appointment by appointment.teller into tellerGroup
                select new TellerAppointment
                {
                    teller = tellerGroup.Key,
                    totalDuration = tellerGroup.Sum(x => x.duration),
                };

            return tellerDuration;
        }

    }
}
