============
 NLog.Azure
============

This package is an extension to [NLog](https://github.com/NLog/NLog/).

This package contains layout-renderes specific to Microsoft Azure Cloud Service

Layout renderers
================

* ${azure-local-resource}


Configuration Syntax
======================

.. code-block:: none

  ${azure-local-resource:name=String:default=String}"


Parameters
==========
Rendering Options
-----------------

* `name` - Cloud Service local resouce name that will be replace local resouce full path string.
* `default` - If Azure Service Runtime not available then this value  will be use.


See [Layout renderers documentation at the NLog wiki](https://github.com/NLog/NLog/wiki/Layout-Renderers)

License
=========
BSD
