namespace PersonalExpenseManager.Domain.Common;

// Marks an entity as belonging to a single user. Ownership is enforced explicitly
// in each query (see AccountQueries) rather than via an automatic EF Core global
// query filter, per the project's "explicit and auditable ownership rules" decision.
public interface IUserOwned
{
    Guid UserId { get; set; }
}
