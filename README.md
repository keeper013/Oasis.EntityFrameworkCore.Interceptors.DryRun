# DryRunInterceptor
## Introduction
**Oasis.EntityFrameworkCore.Interceptors.DryRun** (Referred to as "the library" in following content) library provides an interceptor implementation that allows database operations to be for real or a dry run controlled by a switch.
The library is designed for the following situation
- Both business users and support users have access to a system, while support users have read privilege, while business users have read/write privilege
- Support users sometimes need to poke around in the system including triggering write operations to observe system behaviors for troubleshooting purposes, if the problem lies in the code
- Upon login, login privileges should be retrieved from security tokens or some other ways, then if the user doesn't have write privilege but needs to do such poking arounds, the DbContext they use should be switched to DryRun mode. For users who have write privilege, their DbContext shouldn't turn on DryRun mode.
- With this library, the same set of code handles the business/support user situations easily
- Note that this library only handles database accesses via EntityFramework DbContext. Other database access approaches or external system accesses (e.g. Web API calling) is not covered by this library.
## Design
**IDryRunnable** interface, it provides a switch to turn on/off DryRun mode. Note that this switch must be set before any database write operation is performed if this interceptor is used, and the value can only be set once (Pretty werid use case to run one operation in DryRun mode, then next for real, and vise-versa).

This Interceptor is implemented with 3 basic interceptors: SaveChangesInterceptor, DbCommandInterceptor and TransactionInterceptor. Note that for dry run, all write operations will be suppressed. SaveChangesInterceptor will AcceptAllChanges, DbCommandInterceptor will skip the execution, and TransactionInterceptor will roll back the transaction.

**IDryRunnable** interface also provides a series of events for callers to subscribe based on dry run operations intercepted by the basic interceptors mentioned above.

