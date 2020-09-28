#include "InterfaceMetric.h"

namespace Proton
{
    namespace NetworkUtil
    {
        InterfaceMetric* InterfaceMetric::inst = nullptr;

        InterfaceMetric* InterfaceMetric::instance()
        {
            if (inst == nullptr)
            {
                inst = new InterfaceMetric();
            }

            return inst;
        }

        bool GetInterface(IF_LUID luid, MIB_IPINTERFACE_ROW& iface)
        {
            InitializeIpInterfaceEntry(&iface);
            iface.InterfaceLuid = luid;
            iface.Family = AF_INET;

            return GetIpInterfaceEntry(&iface) == NO_ERROR;
        }

        void SetMetric(IF_LUID luid, ULONG metric, bool autoMetric)
        {
            MIB_IPINTERFACE_ROW iface;
            if (!GetInterface(luid, iface))
            {
                return;
            }

            iface.Metric = metric;
            iface.UseAutomaticMetric = autoMetric;
            iface.SitePrefixLength = 0;

            SetIpInterfaceEntry(&iface);
        }

        bool InterfaceMetric::SetLowestMetric(const IF_LUID luid)
        {
            MIB_IPINTERFACE_ROW iface;
            if (!GetInterface(luid, iface))
            {
                return false;
            }

            SetMetric(luid, 4, false);

            return true;
        }

        bool InterfaceMetric::RestoreDefaultMetric(const IF_LUID luid)
        {
            SetMetric(luid, 0, true);

            return true;
        }
    }
}
