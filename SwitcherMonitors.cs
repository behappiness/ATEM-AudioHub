using BMDSwitcherAPI;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace AudioHub
{
    public delegate void SwitcherEventHandler(object sender, object args);

    class SwitcherMonitor : IBMDSwitcherCallback
    {
        // Events:
        public event SwitcherEventHandler SwitcherDisconnected;

        public SwitcherMonitor()
        {
        }

        void IBMDSwitcherCallback.Notify(_BMDSwitcherEventType eventType, _BMDSwitcherVideoMode coreVideoMode)
        {
            if (eventType == _BMDSwitcherEventType.bmdSwitcherEventTypeDisconnected)
            {
                if (SwitcherDisconnected != null)
                    SwitcherDisconnected(this, null);
            }
        }
    }

    class AudioRoutingOutputMonitor : IBMDSwitcherAudioRoutingOutputCallback
    {
        public event SwitcherEventHandler NameChanged;
        public event SwitcherEventHandler NameDefaultChanged;
        public event SwitcherEventHandler SourceChanged;

        public OutputButton Button { get; set; }

        public AudioRoutingOutputMonitor(OutputButton button)
        {
            Button = button;
        }

        void IBMDSwitcherAudioRoutingOutputCallback.Notify(_BMDSwitcherAudioRoutingOutputEventType eventType)
        {
            switch (eventType)
            {
                case _BMDSwitcherAudioRoutingOutputEventType.bmdSwitcherAudioRoutingOutputEventTypeNameChanged:
                    if (NameChanged != null)
                        NameChanged(this, null);
                    break;
                case _BMDSwitcherAudioRoutingOutputEventType.bmdSwitcherAudioRoutingOutputEventTypeNameDefaultChanged:
                    if (NameDefaultChanged != null)
                        NameDefaultChanged(this, null);
                    break;
                case _BMDSwitcherAudioRoutingOutputEventType.bmdSwitcherAudioRoutingOutputEventTypeSourceChanged:
                    if (SourceChanged != null)
                        SourceChanged(this, null);
                    break;
            }
        }
    }

    class AudioRoutingSourceMonitor : IBMDSwitcherAudioRoutingSourceCallback
    {
        public event SwitcherEventHandler NameChanged;
        public event SwitcherEventHandler NameDefaultChanged;

        public SourceButton Button { get; set; }

        public AudioRoutingSourceMonitor(SourceButton button)
        {
            Button = button;
        }

        void IBMDSwitcherAudioRoutingSourceCallback.Notify(_BMDSwitcherAudioRoutingSourceEventType eventType)
        {
            switch (eventType)
            {
                case _BMDSwitcherAudioRoutingSourceEventType.bmdSwitcherAudioRoutingSourceEventTypeNameChanged:
                    if (NameChanged != null)
                        NameChanged(this, null);
                    break;
                case _BMDSwitcherAudioRoutingSourceEventType.bmdSwitcherAudioRoutingSourceEventTypeNameDefaultChanged:
                    if (NameDefaultChanged != null)
                        NameDefaultChanged(this, null);
                    break;
            }
        }
    }

}
