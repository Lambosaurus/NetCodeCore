NetCode is a library to handle the complexities of real time lossy networking, without requiring a lot of work.
NetCodeTest contains a small test project to demonstrate the functionality of the NetCode library.

A list of planned improvements includes:
	* SyncFlags for extending list and array sizes to two bytes.
	* [Synchronisable] Item[] support
	* Partial updates of compound fields (Lists and Arrays)
	* Automatic registration of Fields and Synchronisable types using Attributes.
	* Light security for authenticating clients
	* Fingerprinting of network definitions so that versions issues can be detected.