namespace Modules.Orders.Domain.Policies;

public static class OrderPolicyConsts
{
    // Policy for admins/managers who can update order status (Ship, Deliver, Cancel)
    public const string ManageOrdersPolicy = "orders:manage";

    // Policy for viewing ANY order (Admin/Support)
    public const string ViewAllOrdersPolicy = "orders:view:all";

    // Policy for viewing YOUR OWN order (Customer)
    // (We implement owner check logic inside the GetOrderById handler for now)
}