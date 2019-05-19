NetCode is a library to handle the complexities of real time lossy networking, without requiring a lot of work.
NetCodeTest contains a small test project to demonstrate the functionality of the NetCode library.

A list of planned improvements include:
- Synchronisable Field Support:
  - Partial updates for Lists and Arrays
  - Support for unordered containers, to minimise effects of removed elements.
  - Nested entitites
  - Auto references, to add entities to the pool when needed.
  - Remove Polling mechanism and have job que specifically for resolving Reference lookups.
- System for automatically breaking large payloads into many small payloads.
- Client/Connection improvements:
  - Light security for authenticating clients.
  - Fingerprinting of NetDefinitions so that versions issues can be detected.
  - Dynamic packet timeouts.
  - Remove UDPConnectionRequestPayload and include more generic solution.
  - Mechanism for deferring and grouping payloads based on transmit priority.
  - Server Blacklisting
- Tools to assist in measuring network usage size.
- General stability testing when recieving garbage payloads.
- Payload encryption options
