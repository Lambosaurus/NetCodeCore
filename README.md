NetCode is a library to handle the complexities of real time lossy networking, without requiring a lot of work.
NetCodeTest contains a small test project to demonstrate the functionality of the NetCode library.

A list of planned improvements include:
- Synchronisable Field Support:
  - SyncFlags for extending list and array sizes to two bytes.
  - Partial updates for Lists and Arrays
  - Allow references to objects in other SyncPools
  - Remove Polling mechanism and have job que specifically for resolving Reference lookups.
- System for automatically breaking large payloads into many small payloads.
- Client/Connection improvements:
  - Light security for authenticating clients.
  - Fingerprinting of NetDefinitions so that versions issues can be detected.
  - Dynamic packet timeouts.
  - Remove UDPConnectionRequestPayload and include more generic solution.
- Tools to assist in measuring network usage size.
- General stability testing when recieving garbage payloads.
