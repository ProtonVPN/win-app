# IPFilter

IPFilter is a thin C++ wrapper for Windows Filtering Platform (WFP).

IPFilter is supported on clients running **Windows 7** and later.

**NOTE: Requires elevated user permissions at run time.**

## Features

* IP packets filtering by layer.
* IP packets filtering by remote IPv4 address.
* IP packets filtering by remote TCP/UDP ports.
* IP packets filtering by application.
* IP packets filtering by remote IPv4 network address.
* IP packets filtering by network interface.

## Filter Arbitration

Each layer is divided into sub-layers ordered by weight. The filter arbitration algorithm is applied to all sub-layers within a layer.

Below is an example how filter arbitration works:

```
+---------------------------------------------------+
|                                                   |
|    Filter   | Weight |   Action    |   Sublayer   |
| ------------------------------------------------- |
| Filter (A)  |    2   | Soft Block  | Sublayer (A) |
| Filter (B)  |    1   | Permit      | Sublayer (A) |
| Filter (C)  |    1   | Permit      | Sublayer (B) |
|                                                   |
+---------------------------------------------------+

                          |  #1 Select one matching filter
                          |     from each sublayer which
                          |     has the highest weight
                          |
                          v

+-------------------------------------------+
|                                           |
|    Filter  | Sublayer Weight |   Action   |
| ----------------------------------------- |
| Filter (A) |        5        | Soft Block |
| Filter (C) |        4        | Permit     |
|                                           |
+-------------------------------------------+

                          |  #2 Make a final decision
                          |     depending on sublayer weight
                          |     and action type
                          |
                          v

                        Permit
```

### Filter arbitration within a sub-layer

1. Computes the list of matching filters ordered by weight. Higher the weight higher the priority.
2. Returns filter with a highest weight.

### Filter arbitration within a layer

1. Performs filter arbitration at every sub-layer.
2. Returns resulting action based on the policy rules:
    * Actions are evaluated in priority order of sub-layers from highest priority to lowest.
    * Action `Block` overrides action `Permit`.
    * `Soft` version of `Block` or `Permit` actions allows lower priority sub-layer override it's action.


[WFP]:https://docs.microsoft.com/en-us/windows/desktop/fwp/fwp-reference
