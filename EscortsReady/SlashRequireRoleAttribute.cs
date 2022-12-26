// See https://aka.ms/new-console-template for more information

using DSharpPlus.SlashCommands;
using System.Collections.ObjectModel;

namespace EscortsReady
{
    /// <summary>
    /// Defines that usage of this command is restricted to members with specified role.
    /// </summary>
    public class SlashRequireRoleAttribute : SlashCheckBaseAttribute
    {
        /// <summary>
        /// Gets the name of the role required to execute this command.
        /// </summary>
        public IReadOnlyList<string> RoleNames { get; }

        /// <summary>
        /// Gets the role checking mode. Refer to <see cref="RoleCheckMode"/> for more information.
        /// </summary>
        public RoleCheckMode CheckMode { get; }

        /// <summary>
        /// Defines that usage of this command is restricted to members with specified role.
        /// </summary>
        /// <param name="checkMode">Role checking mode.</param>
        /// <param name="roleNames">Names of the role to be verified by this check.</param>
        public SlashRequireRoleAttribute(RoleCheckMode checkMode, params string[] roleNames)
        {
            this.CheckMode = checkMode;
            this.RoleNames = new ReadOnlyCollection<string>(roleNames);
        }
        public override async Task<bool> ExecuteChecksAsync(InteractionContext ctx)
        {
            var isOwner = false; // await new SlashRequireOrbitalOwnerAttribute().ExecuteChecksAsync(ctx, true);
            if (isOwner) return await Task.FromResult(true);

            if (ctx.Guild == null || ctx.Member == null)
                return await Task.FromResult(false);

            var rns = ctx.Member.Roles.Select(xr => xr.Name).ToList();
            var rnc = rns.Count();
            var ins = rns.Intersect(this.RoleNames, StringComparer.OrdinalIgnoreCase);
            var inc = ins.Count();

            var result = this.CheckMode switch
            {
                RoleCheckMode.All => Task.FromResult(this.RoleNames.Count == inc),
                RoleCheckMode.SpecifiedOnly => Task.FromResult(rnc == inc),
                RoleCheckMode.None => Task.FromResult(inc == 0),
                _ => Task.FromResult(inc > 0),

            };
            if (!result.Result)
                await ctx.CreateResponseAsync("Sorry you have not the correct permission to use this command", true);
            return await Task.FromResult(result.Result);
        }

    }
    /// <summary>
    /// Specifies how does <see cref="SlashRequireRoleAttribute"/> check for roles.
    /// </summary>
    public enum RoleCheckMode
    {
        /// <summary>
        /// Member is required to have any of the specified roles.
        /// </summary>
        Any,

        /// <summary>
        /// Member is required to have all of the specified roles.
        /// </summary>
        All,

        /// <summary>
        /// Member is required to have exactly the same roles as specified; no extra roles may be present.
        /// </summary>
        SpecifiedOnly,

        /// <summary>
        /// Member is required to have none of the specified roles.
        /// </summary>
        None
    }
}