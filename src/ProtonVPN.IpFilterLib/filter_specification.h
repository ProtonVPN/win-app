#pragma once
#include <fwptypes.h>
#include <fwpmu.h>

#include <vector>
#include <stdexcept>

#include "condition.h"

namespace ipfilter
{
    class FilterSpecification
    {
    public:
        FilterSpecification();

        void block();

        void permit();

        void hard();

        void soft();

        void persistent();

        void callout(GUID* calloutKey);

        void setWeight(unsigned int weight);

        FWPM_ACTION0 getAction() const;

        UINT32 getFlags() const;

        void addCondition(const condition::Condition& condition);

        std::vector<condition::Condition> getConditions() const;

        FWP_VALUE0 getWeight() const;

    private:
        FWPM_ACTION0 action;

        UINT32 flags;

        FWP_VALUE0 weight;

        std::vector<condition::Condition> conditions;
    };
}
