# vixen

A LucidVM monitor written in C# and targeting **VMware hypervisors** via the VIX API.

Provides a minimal interface to the gateway server for discovering a VM's VNC address and abstracting operations like resets and "virus farm" style file pushes. **QEMU/KVM support not implemented**, but *is* planned as a separate monitor implementation (optionally in-process with the gateway to simplify deployment).

This software is currently **pre-alpha**, and at this stage, **I will not assist in using it**. Please wait until a more mature release!
