using System;
using System.Collections.Generic;
using System.Windows.Forms;

using Centralizador.Models.ApiCEN;

namespace Centralizador.WinApp.GUI
{
    public partial class FormMain : Form
    {
        public FormMain()
        {
            InitializeComponent();
        }

        private void FormMain_Load(object sender, EventArgs e)
        {

            ResultAgent agent = new ResultAgent
            {
                Email = "german.gomez@cvegroup.com"
            };
            agent = Agent.GetAgetByEmail(agent);

            IList<ResultParticipant> participants = new List<ResultParticipant>();
            foreach (ResultParticipant item in agent.Participants)
            {
                participants.Add(Participant.GetParticipantById(item));
            }


        }
    }
}
