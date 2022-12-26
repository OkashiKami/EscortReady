// See https://aka.ms/new-console-template for more information

using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;
using DSharpPlus;
using System.Diagnostics;
using System.Security.Principal;
using VRChat.API.Model;
using DSharpPlus.Interactivity.Extensions;

namespace PermissionEx
{

    [SlashCommandGroup("vrc", "VRChat Module")]
    public class SlashVRChatCommands : ApplicationCommandModule
    {
        [SlashCommand("link", "Links your vrchat accoutn with the discord server")]
        public async Task Link(InteractionContext ctx, [RemainingText, Option("VRChatInfo", "This would be your vrchat Profile URL, _vrcUsername, or Displayname, recommend: profile url")] string? vrcinfo)
        {
            await ctx.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource);
            await LoggerEx.LogAsync(ctx);
            List<User> users = new List<User>();
            try
            {
                if (vrcinfo.StartsWith("https://vrchat.com/"))
                {
                    var u = await VRChatService.GetUserByProfileURLAsync(vrcinfo);
                    if (u != null) users.Add(u);
                }
                var u2 = await VRChatService.GetUserByUsernameAsync(vrcinfo);
                if (u2 != null) users.Add(u2);

                var u3 = await VRChatService.GetUserByDisplaynameAsync(vrcinfo);
                if (u3 != null) users.Add(u3);

            }
            catch { }
            if (users.Count <= 0)
            {
                var wb = new DiscordWebhookBuilder();
                var emb3 = new DiscordEmbedBuilder();
                emb3.WithDescription("Sorry no account was found, Please try again.");
                wb.AddEmbed(emb3);
                await ctx.EditResponseAsync(wb);
                return;
            }
            bool set = false;
            foreach (var user in users)
            {
                var emb = new DiscordWebhookBuilder();
                var button1 = new DiscordButtonComponent(ButtonStyle.Success, "_yes", "Yes");
                var button2 = new DiscordButtonComponent(ButtonStyle.Danger, "_no", "No");
                var e = new DiscordEmbedBuilder();
                e.Title = $"{user.DisplayName} ({user.Username})";
                e.Author = new DiscordEmbedBuilder.EmbedAuthor()
                {
                    Name = $"{user.StatusDescription} ({user.Status})"
                };
                e.Thumbnail = new DiscordEmbedBuilder.EmbedThumbnail()
                {
                    Width = 64,
                    Height = 64,
                    Url = (!string.IsNullOrEmpty(user.UserIcon) ? user.UserIcon.Substring(0, user.UserIcon.Length - 1) + ".png" : user.CurrentAvatarImageUrl)
                };
                e.Description += user.Bio;
                //e.AddField("Location", await Utils.GetLocation(user.Location));
                foreach (var item in user.BioLinks)
                {
                    if (!string.IsNullOrEmpty(item))
                        e.AddField(Utils.GetDomain(item), item, true);
                }
                e.AddField("Date Joined", user.DateJoined.ToShortDateString());
                e.AddField("Avatar", "** **");
                e.ImageUrl = user.CurrentAvatarThumbnailImageUrl;
                var emb2 = new DiscordEmbedBuilder();
                emb2.WithDescription("Is this your account?");
                emb.AddEmbed(e);
                emb.AddEmbed(emb2);
                emb.AddComponents(button2, button1);
                var msg = await ctx.EditResponseAsync(emb);
                var btn = await msg.WaitForButtonAsync(ctx.User, TimeSpan.FromMinutes(2));
                var dwb = new DiscordWebhookBuilder();
                var em = new DiscordEmbedBuilder();
                em.WithDescription(":x: Sorry the action has timed out");
                dwb.AddEmbed(em);
                if (btn.TimedOut) await ctx.EditResponseAsync(dwb);
                else
                {
                    var wb = new DiscordWebhookBuilder();
                    var emb3 = new DiscordEmbedBuilder();
                    switch (btn.Result.Id)
                    {
                        case "_yes":
                            await btn.Result.Interaction.CreateResponseAsync(InteractionResponseType.DeferredMessageUpdate);
                            var member = await Member.LoadAsync(ctx.Guild, x => x.id == ctx.Member.Id);

                            member.guildid = ctx.Guild.Id;
                            member.id = ctx.Member.Id;
                            member.vrcuserid = user.Id;
                            member.vrcdisplayname = user.DisplayName;

                            var _role = ctx.Guild.Roles.ToList().Find(x => x.Value.Name.Equals("VRCLinked", StringComparison.OrdinalIgnoreCase));
                            var role = _role.Value != null ? _role.Value : null;
                            if (!ctx.Member.Roles.Contains(role))
                            {
                                emb3.WithDescription($"{ctx.Member.Username} vrchat _vrcUsername has been added and their discord is now linked.");
                                wb.AddEmbed(emb3);
                                await ctx.EditResponseAsync(wb);
                                await (ctx.Member).GrantRoleAsync(role);
                                await Member.SaveAsync(ctx.Guild, member);
                                await VRCAddThem(ctx, ctx.Member);
                            }
                            else
                            {
                                await Member.SaveAsync(ctx.Guild, member);
                                await VRCAddThem(ctx, ctx.Member);
                                emb3.WithDescription($"{ctx.Member.Username} vrchat _vrcUsername has been Updated!");
                                wb.AddEmbed(emb3);
                                await ctx.EditResponseAsync(wb);
                            }
                            wb = new DiscordWebhookBuilder();
                            emb3 = new DiscordEmbedBuilder();
                            emb3.WithDescription("Okay, Their discrod account has been linked.");
                            wb.AddEmbed(emb3);
                            await ctx.EditResponseAsync(wb);
                            //await UpdatePermissionsFile(ctx, true);
                            await member.CloseAsync(ctx.Guild);
                            set = true;
                            break;
                        case "_no":
                            set = false;
                            break;
                    }
                }
                if (set) break;
            }
            if (!set)
            {
                var wb = new DiscordWebhookBuilder();
                var emb3 = new DiscordEmbedBuilder();
                emb3.WithDescription("Okay, Please check your info or try a differnet method.");
                wb.AddEmbed(emb3);
                await ctx.EditResponseAsync(wb);
            }
        }
       
        //[SlashRequireOrbitalStaff]
        [SlashCommand("linkthem", "Links the given member's vrchat with the discord server")]
        public async Task LinkThem(InteractionContext ctx, [Option("DiscordUser", "The member you want to register")] DiscordUser member, [RemainingText, Option("VRChatInfo", "This would be your vrchat Profile URL, _vrcUsername, or Displayname, recommend: profile url")] string? vrcinfo = null)
        {
            await ctx.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource);
            await LoggerEx.LogAsync(ctx);
            var _member = await Member.LoadAsync(ctx.Guild, x => x.id == member.Id);
            List<User> users = new List<User>();
            try
            {
                if (vrcinfo.StartsWith("https://vrchat.com/"))
                {
                    var u = await VRChatService.GetUserByProfileURLAsync(vrcinfo);
                    if (u != null) users.Add(u);
                }
                var u2 = await VRChatService.GetUserByUsernameAsync(vrcinfo);
                if (u2 != null) users.Add(u2);

                var u3 = await VRChatService.GetUserByDisplaynameAsync(vrcinfo);
                if (u3 != null) users.Add(u3);

            }
            catch { }
            if (users.Count <= 0)
            {
                var wb = new DiscordWebhookBuilder();
                var emb3 = new DiscordEmbedBuilder();
                emb3.WithDescription("Sorry no account was found, Please try again.");
                wb.AddEmbed(emb3);
                await ctx.EditResponseAsync(wb);
                return;
            }
            bool set = false;
            foreach (var user in users)
            {
                var emb = new DiscordWebhookBuilder();
                var button1 = new DiscordButtonComponent(ButtonStyle.Success, "_yes", "Yes");
                var button2 = new DiscordButtonComponent(ButtonStyle.Danger, "_no", "No");
                var e = new DiscordEmbedBuilder();
                e.Title = $"{user.DisplayName} ({user.Username})";
                e.Author = new DiscordEmbedBuilder.EmbedAuthor()
                {
                    Name = $"{user.StatusDescription} ({user.Status})"
                };
                e.Thumbnail = new DiscordEmbedBuilder.EmbedThumbnail()
                {
                    Width = 64,
                    Height = 64,
                    Url = (!string.IsNullOrEmpty(user.UserIcon) ? user.UserIcon.Substring(0, user.UserIcon.Length - 1) + ".png" : user.CurrentAvatarImageUrl)
                };
                e.Description += user.Bio;
                //e.AddField("Location", await Utils.GetLocation(user.Location));
                foreach (var item in user.BioLinks)
                {
                    if (!string.IsNullOrEmpty(item))
                        e.AddField(Utils.GetDomain(item), item, true);
                }
                e.AddField("Date Joined", user.DateJoined.ToShortDateString());
                e.AddField("Avatar", "** **");
                e.ImageUrl = user.CurrentAvatarThumbnailImageUrl;
                var emb2 = new DiscordEmbedBuilder();
                emb2.WithDescription("Is this their account?");
                emb.AddEmbed(e);
                emb.AddEmbed(emb2);
                emb.AddComponents(button2, button1);
                var msg = await ctx.EditResponseAsync(emb);
                var btn = await msg.WaitForButtonAsync(ctx.User, TimeSpan.FromMinutes(2));
                var dwb = new DiscordWebhookBuilder();
                var em = new DiscordEmbedBuilder();
                em.WithDescription(":x: Sorry the action has timed out");
                dwb.AddEmbed(em);
                if (btn.TimedOut) await ctx.EditResponseAsync(dwb);
                else
                {
                    var wb = new DiscordWebhookBuilder();
                    var emb3 = new DiscordEmbedBuilder();
                    switch (btn.Result.Id)
                    {
                        case "_yes":
                            await btn.Result.Interaction.CreateResponseAsync(InteractionResponseType.DeferredMessageUpdate);
                            _member.guildid = ctx.Guild.Id;
                            _member.id = ctx.Member.Id;
                            _member.vrcuserid = user.Id;
                            _member.vrcdisplayname = user.DisplayName;


                            var _role = ctx.Guild.Roles.ToList().Find(x => x.Value.Name.Equals("VRCLinked", StringComparison.OrdinalIgnoreCase));
                            var role = _role.Value != null ? _role.Value : null;
                            if (!((DiscordMember)member).Roles.Contains(role))
                            {
                                emb3.WithDescription($"{((DiscordMember)member).Username} vrchat _vrcUsername has been added and their discord is now linked.");
                                wb.AddEmbed(emb3);
                                await ctx.EditResponseAsync(wb);
                                await ((DiscordMember)member).GrantRoleAsync(role);

                                var vrcuser = await VRChatService.SendFriendRequestAsync((DiscordMember)member);
                                var _dwb = new DiscordWebhookBuilder();
                                var _em = new DiscordEmbedBuilder();
                                _em.WithDescription($"Friend request has ben sent to **{vrcuser?.DisplayName}**");
                                _dwb.AddEmbed(_em);
                                await ctx.EditResponseAsync(_dwb);
                            }
                            else
                            {
                                var vrcuser = await VRChatService.SendFriendRequestAsync((DiscordMember)member);
                                var _dwb = new DiscordWebhookBuilder();
                                var _em = new DiscordEmbedBuilder();
                                _em.WithDescription($"Friend request has ben sent to **{vrcuser?.DisplayName}**");
                                _dwb.AddEmbed(_em);
                                await ctx.EditResponseAsync(_dwb);


                                emb3.WithDescription($"{((DiscordMember)member).Username} vrchat _vrcUsername has been Updated!");
                                wb.AddEmbed(emb3);
                                await ctx.EditResponseAsync(wb);
                            }
                            wb = new DiscordWebhookBuilder();
                            emb3 = new DiscordEmbedBuilder();
                            emb3.WithDescription("Okay, Their discrod account has been linked.");
                            wb.AddEmbed(emb3);
                            await ctx.EditResponseAsync(wb);
                            await _member.CloseAsync(ctx.Guild);
                            //await UpdatePermissionsFile(ctx, true);
                            set = true;
                            break;
                        case "_no":
                            set = false;
                            break;
                    }
                }
                if (set) break;
            }
            if (!set)
            {
                var wb = new DiscordWebhookBuilder();
                var emb3 = new DiscordEmbedBuilder();
                emb3.WithDescription("Okay, Please check your info or try a differnet method.");
                wb.AddEmbed(emb3);
                await ctx.EditResponseAsync(wb);
            }
        }



        [SlashCommand("unlink", "Unlinks your vrchat account with the discord server")]
        public async Task Unlink(InteractionContext ctx)
        {
            await ctx.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource);
            var member = await Member.LoadAsync(ctx.Guild, x => x.id == ctx.Member.Id);
            var user = await VRChatService.GetUserByProfileURLAsync(member.vrcuserid);
            var emb = new DiscordWebhookBuilder();
            var button1 = new DiscordButtonComponent(ButtonStyle.Success, "_yes", "Yes");
            var button2 = new DiscordButtonComponent(ButtonStyle.Danger, "_no", "No");
            var e = new DiscordEmbedBuilder();
            e.Title = $"{user.DisplayName} ({user.Username})";
            e.Author = new DiscordEmbedBuilder.EmbedAuthor()
            {
                Name = $"{user.StatusDescription} ({user.Status})"
            };
            e.Thumbnail = new DiscordEmbedBuilder.EmbedThumbnail()
            {
                Width = 64,
                Height = 64,
                Url = (!string.IsNullOrEmpty(user.UserIcon) ? user.UserIcon.Substring(0, user.UserIcon.Length - 1) + ".png" : user.CurrentAvatarImageUrl)
            };
            e.Description += user.Bio;
            //e.AddField("Location", await Utils.GetLocation(user.Location));
            foreach (var item in user.BioLinks)
            {
                if (!string.IsNullOrEmpty(item))
                    e.AddField(Utils.GetDomain(item), item, true);
            }
            e.AddField("Date Joined", user.DateJoined.ToShortDateString());
            e.AddField("Avatar", "** **");
            e.ImageUrl = user.CurrentAvatarThumbnailImageUrl;
            var emb2 = new DiscordEmbedBuilder();
            emb2.WithDescription("Are you sure you would like to unlink your account?");
            emb.AddEmbed(e);
            emb.AddEmbed(emb2);
            emb.AddComponents(button2, button1);
            var msg = await ctx.EditResponseAsync(emb);
            var btn = await msg.WaitForButtonAsync(ctx.User, TimeSpan.FromMinutes(2));
            var dwb = new DiscordWebhookBuilder();
            var em = new DiscordEmbedBuilder();
            em.WithDescription(":x: Sorry the action has timed out");
            dwb.AddEmbed(em);
            if (btn.TimedOut) await ctx.EditResponseAsync(dwb);
            else
            {
                var wb = new DiscordWebhookBuilder();
                var emb3 = new DiscordEmbedBuilder();
                switch (btn.Result.Id)
                {
                    case "_yes":
                        await btn.Result.Interaction.CreateResponseAsync(InteractionResponseType.DeferredMessageUpdate);
                        member.vrcuserid = null;
                        member.vrcdisplayname = null;

                        var _role = ctx.Guild.Roles.ToList().Find(x => x.Value.Name.Equals("VRCLinked", StringComparison.OrdinalIgnoreCase));
                        var role = _role.Value != null ? _role.Value : null;
                        if (ctx.Member.Roles.Contains(role))
                        {
                            emb3.WithDescription($"{ctx.Member.Username} vrchat _vrcUsername has been removed and their discord is no longer linked.");
                            wb.AddEmbed(emb3);
                            await ctx.EditResponseAsync(wb);
                            await (ctx.Member).RevokeRoleAsync(role);
                            await VRCUnFriendThem(ctx, ctx.Member);
                        }
                        wb = new DiscordWebhookBuilder();
                        emb3 = new DiscordEmbedBuilder();
                        emb3.WithDescription("Okay, Their discrod account has been linked.");
                        wb.AddEmbed(emb3);
                        await ctx.EditResponseAsync(wb);
                        await member.CloseAsync(ctx.Guild);
                        //await UpdatePermissionsFile(ctx, true);
                        break;
                    case "_no":
                        wb = new DiscordWebhookBuilder();
                        emb3 = new DiscordEmbedBuilder();
                        emb3.WithDescription("Okay!");
                        wb.AddEmbed(emb3);
                        await ctx.EditResponseAsync(wb);
                        break;
                }
            }
        }
        //[SlashRequireOrbitalStaff]
        [SlashCommand("Unlinkthem", "Unlinks the given member's vrchat account with the discord server")]
        public async Task UnlinkThem(InteractionContext ctx, [Option("DiscordUser", "The member you want to register")] DiscordUser member)
        {
            await ctx.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource);
            var _member = await Member.LoadAsync(ctx.Guild, x => x.id == member.Id);
            var user = await VRChatService.GetUserByProfileURLAsync(_member.vrcuserid);
            var emb = new DiscordWebhookBuilder();
            var button1 = new DiscordButtonComponent(ButtonStyle.Success, "_yes", "Yes");
            var button2 = new DiscordButtonComponent(ButtonStyle.Danger, "_no", "No");
            var e = new DiscordEmbedBuilder();
            e.Title = $"{user?.DisplayName} ({user?.Username})";
            e.Author = new DiscordEmbedBuilder.EmbedAuthor()
            {
                Name = $"{user?.StatusDescription} ({user?.Status})"
            };
            e.Thumbnail = new DiscordEmbedBuilder.EmbedThumbnail()
            {
                Width = 64,
                Height = 64,
                Url = (!string.IsNullOrEmpty(user.UserIcon) ? user.UserIcon.Substring(0, user.UserIcon.Length - 1) + ".png" : user.CurrentAvatarImageUrl)
            };
            e.Description += user.Bio;
            //e.AddField("Location", await Utils.GetLocation(user.Location));
            foreach (var item in user.BioLinks)
            {
                if (!string.IsNullOrEmpty(item))
                    e.AddField(Utils.GetDomain(item), item, true);
            }
            e.AddField("Date Joined", user.DateJoined.ToShortDateString());
            e.AddField("Avatar", "** **");
            e.ImageUrl = user.CurrentAvatarThumbnailImageUrl;
            var emb2 = new DiscordEmbedBuilder();
            emb2.WithDescription("Are you sure you would like to unlink their account?");
            emb.AddEmbed(e);
            emb.AddEmbed(emb2);
            emb.AddComponents(button2, button1);
            var msg = await ctx.EditResponseAsync(emb);
            var btn = await msg.WaitForButtonAsync(ctx.User, TimeSpan.FromMinutes(2));
            var dwb = new DiscordWebhookBuilder();
            var em = new DiscordEmbedBuilder();
            em.WithDescription(":x: Sorry the action has timed out");
            dwb.AddEmbed(em);
            if (btn.TimedOut) await ctx.EditResponseAsync(dwb);
            else
            {
                var wb = new DiscordWebhookBuilder();
                var emb3 = new DiscordEmbedBuilder();
                switch (btn.Result.Id)
                {
                    case "_yes":
                        await btn.Result.Interaction.CreateResponseAsync(InteractionResponseType.DeferredMessageUpdate);
                        _member.vrcuserid = null;
                        _member.vrcdisplayname = null;
                        var _role = ctx.Guild.Roles.ToList().Find(x => x.Value.Name.Equals("VRCLinked", StringComparison.OrdinalIgnoreCase));
                        var role = _role.Value != null ? _role.Value : null;
                        if (((DiscordMember)member).Roles.Contains(role))
                        {
                            emb3.WithDescription($"{member.Username} vrchat _vrcUsername has been removed and their discord is no longer linked.");
                            wb.AddEmbed(emb3);
                            await ctx.EditResponseAsync(wb);
                            await (((DiscordMember)member)).RevokeRoleAsync(role);
                            await VRCUnFriendThem(ctx, ((DiscordMember)member));
                        }
                        wb = new DiscordWebhookBuilder();
                        emb3 = new DiscordEmbedBuilder();
                        emb3.WithDescription("Okay, Their discrod account has been unlinked.");
                        wb.AddEmbed(emb3);
                        await ctx.EditResponseAsync(wb);
                        await _member.CloseAsync(ctx.Guild);
                        //await UpdatePermissionsFile(ctx, true);
                        break;
                    case "_no":
                        wb = new DiscordWebhookBuilder();
                        emb3 = new DiscordEmbedBuilder();
                        emb3.WithDescription("Okay!");
                        wb.AddEmbed(emb3);
                        await ctx.EditResponseAsync(wb);
                        break;
                }
            }
        }


        [SlashRequireRole(RoleCheckMode.Any, "VRCLinked")]
        [SlashCommand("whoami", "Give you information about your vrc account")]
        public async Task WhoAmI(InteractionContext ctx)
        {
            await ctx.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource);
            await LoggerEx.LogAsync(ctx);
            var _member = await Member.LoadAsync(ctx.Guild, x => x.id == ctx.Member.Id);
            var vrcuser = await VRChatService.GetUserByProfileURLAsync(_member.vrcuserid) ?? await VRChatService.GetUserByUsernameAsync(_member.vrcdisplayname) ?? await VRChatService.GetUserByDisplaynameAsync(_member.vrcdisplayname);
            if (vrcuser != default)
            {
                var e = new DiscordEmbedBuilder();
                e.Title = $"{vrcuser.DisplayName} ({vrcuser.Username})";
                e.Author = new DiscordEmbedBuilder.EmbedAuthor()
                {
                    Name = $"{vrcuser.StatusDescription} ({vrcuser.Status})"
                };
                e.Thumbnail = new DiscordEmbedBuilder.EmbedThumbnail()
                {
                    Width = 64,
                    Height = 64,
                    Url = (!string.IsNullOrEmpty(vrcuser.UserIcon) ? vrcuser.UserIcon.Substring(0, vrcuser.UserIcon.Length - 1) + ".png" : vrcuser.CurrentAvatarImageUrl)
                };
                e.Description += vrcuser.Bio;
                e.AddField("Location", await Utils.GetLocation(vrcuser.Location));
                foreach (var item in vrcuser.BioLinks)
                    e.AddField(Utils.GetDomain(item), item, true);
                e.AddField("Date Joined", vrcuser.DateJoined.ToShortDateString());
                e.AddField("Avatar", "** **");
                e.ImageUrl = vrcuser.CurrentAvatarThumbnailImageUrl;
                await ctx.EditResponseAsync(new DiscordWebhookBuilder().AddEmbed(e.Build()));
            }
            else await ctx.DeleteResponseAsync(); 

        }
        [SlashCommand("whoarethey", "Give you information about their discord or query")]
        public async Task WhoAreThey(InteractionContext ctx, [Option("DiscordUser", "The member you would like to get info on")] DiscordUser member = null, [Option("Query", "The member you would like to get info on")] string query = null)
        {
            await ctx.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource);
            await LoggerEx.LogAsync(ctx);
            var role = ctx.Guild.Roles.FirstOrDefault(x => x.Value.Name == "VRCLinked").Value;
            if (!((DiscordMember)member).Roles.Contains(role))
            {
                await ctx.EditResponseAsync(new DiscordWebhookBuilder().AddEmbed(new DiscordEmbedBuilder().WithDescription("Sorry they have not yet registered their VRChat name with me yet.")));
                return;
            }


            if (member != null)
            {
                var _member = await Member.LoadAsync(ctx.Guild, x => x.id == member.Id);
                var vrcuser = await VRChatService.GetUserByProfileURLAsync(_member.vrcuserid) ?? await VRChatService.GetUserByUsernameAsync(_member.vrcdisplayname) ?? await VRChatService.GetUserByDisplaynameAsync(_member.vrcdisplayname);
                if (vrcuser != default)
                {
                    var e = new DiscordEmbedBuilder();
                    e.Title = $"{vrcuser.DisplayName} ({vrcuser.Username})";
                    e.Author = new DiscordEmbedBuilder.EmbedAuthor()
                    {
                        Name = $"{vrcuser.StatusDescription} ({vrcuser.Status})"
                    };
                    e.Thumbnail = new DiscordEmbedBuilder.EmbedThumbnail()
                    {
                        Width = 64,
                        Height = 64,
                        Url = (!string.IsNullOrEmpty(vrcuser.UserIcon) ? vrcuser.UserIcon.Substring(0, vrcuser.UserIcon.Length - 1) + ".png" : vrcuser.CurrentAvatarImageUrl)
                    };
                    e.Description += vrcuser.Bio;
                    e.AddField("Location", await Utils.GetLocation(vrcuser.Location));
                    foreach (var item in vrcuser.BioLinks)
                    {
                        if (!string.IsNullOrEmpty(item))
                            e.AddField(Utils.GetDomain(item), item, true);
                    }
                    e.AddField("Date Joined", vrcuser.DateJoined.ToShortDateString());
                    e.AddField("Avatar", "** **");
                    e.ImageUrl = vrcuser.CurrentAvatarThumbnailImageUrl;
                    await ctx.EditResponseAsync(new DiscordWebhookBuilder().AddEmbed(e.Build()));
                }
                else await ctx.DeleteResponseAsync();
            }
            else if (!string.IsNullOrEmpty(query))
            {
                var vrcuser = await VRChatService.GetUserByUsernameAsync(query) ?? await VRChatService.GetUserByDisplaynameAsync(query);
                if (vrcuser != default)
                {
                    var e = new DiscordEmbedBuilder();
                    e.Title = $"{vrcuser.DisplayName} ({vrcuser.Username})";
                    e.Author = new DiscordEmbedBuilder.EmbedAuthor()
                    {
                        Name = $"{vrcuser.StatusDescription} ({vrcuser.Status})"
                    };
                    e.Thumbnail = new DiscordEmbedBuilder.EmbedThumbnail()
                    {
                        Width = 64,
                        Height = 64,
                        Url = vrcuser.UserIcon ?? vrcuser.CurrentAvatarImageUrl
                    };
                    e.Description += vrcuser.Bio;
                    e.AddField("Location", await Utils.GetLocation(vrcuser.Location));
                    foreach (var item in vrcuser.BioLinks)
                    {
                        if (!string.IsNullOrEmpty(item))
                            e.AddField(Utils.GetDomain(item), item, true);
                    }
                    e.AddField("Date Joined", vrcuser.DateJoined.ToShortDateString());
                    e.AddField("Avatar", "** **");
                    e.ImageUrl = vrcuser.CurrentAvatarThumbnailImageUrl;
                    await ctx.EditResponseAsync(new DiscordWebhookBuilder().AddEmbed(e.Build()));
                }
                else await ctx.DeleteResponseAsync();
            }
        }



        [SlashRequireRole(RoleCheckMode.Any, "VRCLinked")]
        [SlashCommand("inv-self", "Give you information about your vrc account")]
        public async Task InviteSelf(InteractionContext ctx)
        {
            await ctx.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource);
            var role = ctx.Guild.Roles.FirstOrDefault(x => x.Value.Name == "VRCLinked").Value;
            if (!ctx.Member.Roles.Contains(role))
            {
                var emb = new DiscordEmbedBuilder();
                emb.WithColor(DiscordColor.Red);
                emb.WithAuthor("VRChat");
                emb.WithDescription("Sorry you have not registered your VRChat name with me yet. :x:");

                var builder = new DiscordWebhookBuilder();
                builder.AddEmbed(emb);

                await ctx.EditResponseAsync(builder);
                return;
            }
            var _member = await Member.LoadAsync(ctx.Guild, x => x.id == ctx.Member.Id);
            var vrcuser = await VRChatService.GetUserByProfileURLAsync(_member.vrcuserid) ?? await VRChatService.GetUserByUsernameAsync(_member.vrcdisplayname) ?? await VRChatService.GetUserByDisplaynameAsync(_member.vrcdisplayname);
            if (vrcuser != default)
                await VRChatService.SendEventInviteAsync(ctx.Guild, _member.id, vrcuser, async () =>
                {
                    var emb = new DiscordEmbedBuilder();
                    emb.WithColor(DiscordColor.Green);
                    emb.WithAuthor("VRChat");
                    emb.WithDescription($"World Invite has ben sent to **{vrcuser.DisplayName}** :white_check_mark: ");

                    var builder = new DiscordWebhookBuilder();
                    builder.AddEmbed(emb);
                    await ctx.EditResponseAsync(builder);
                });
        }
        
        [SlashRequireRole(RoleCheckMode.Any, "VRCLinked")]
        [SlashCommand("inv-them", "Invites the member to the world evetnt instance")]
        public async Task InviteMe(InteractionContext ctx, [Option("Member", "The member you would like to get info on")] DiscordUser member)
        {
            var role = ctx.Guild.Roles.FirstOrDefault(x => x.Value.Name == "VRCLinked").Value;
            if (!((DiscordMember)member).Roles.Contains(role))
            {
                await ctx.CreateResponseAsync("Sorry you have not registered your VRChat name with me yet.");
                return;
            }
            var _member = await Member.LoadAsync(ctx.Guild, x => x.id == member.Id);
            var vrcuser = await VRChatService.GetUserByProfileURLAsync(_member.vrcuserid) ?? await VRChatService.GetUserByUsernameAsync(_member.vrcdisplayname) ?? await VRChatService.GetUserByDisplaynameAsync(_member.vrcdisplayname);
            if (vrcuser != default)
                await VRChatService.SendEventInviteAsync(ctx.Guild, _member.id, vrcuser, async () => await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder()
                {
                    Content = $"World Invite has ben sent to **{vrcuser.DisplayName}**"
                }));
        }
        //[SlashRequireOrbitalManagement]
        
        [SlashCommand("inv-role", "Invites all members to the world evetnt instance who have th desire role")]
        public async Task InviteRole(InteractionContext ctx, [Option("Role", "The role fof the members you would like to get info on")] DiscordRole role)
        {
            var _role = ctx.Guild.Roles.FirstOrDefault(x => x.Value.Name == "VRCLinked").Value;
            var members = await ctx.Guild.GetAllMembersAsync();
            var _members = members.ToList().Where(x => x.Roles.Contains(role));
            var invc = 0;
            await ctx.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource);
            foreach (var _member in _members)
            {
                if (invc > 5)
                {
                    await Task.Delay(30000);
                    invc = 0;
                }
                if (!_member.Roles.Contains(role))
                {
                    //await ctx.CreateResponseAsync($"Sorry {_member.Username}:{_member.Discriminator} have not registered your VRChat name with me yet.");
                }
                else
                {
                    var mem = await Member.LoadAsync(ctx.Guild, x => x.id == _member.Id);
                    var vrcuser = await VRChatService.GetUserByProfileURLAsync(mem.vrcuserid) ?? await VRChatService.GetUserByUsernameAsync(mem.vrcdisplayname) ?? await VRChatService.GetUserByDisplaynameAsync(mem.vrcdisplayname);
                    if (vrcuser != default)
                    {
                        await VRChatService.SendEventInviteAsync(ctx.Guild, _member.Id, vrcuser);
                        invc++;
                    }
                }
            }
            var dwb = new DiscordWebhookBuilder();
            var demb = new DiscordEmbedBuilder();
            demb.WithDescription($"World Invite has ben sent to **{_role.Mention}**");
            dwb.AddEmbed(demb);
            await ctx.EditResponseAsync(dwb);
        }
        
        [SlashCommand("inv", "Invites the member to the world evetnt instance")]
        [SlashRequireRole(RoleCheckMode.Any, "VRCLinked")]
        public async Task Invite(InteractionContext ctx, [Option("Query", "The member you would like to get info on")] string query)
        {
            var vrcuser = await VRChatService.GetUserByUsernameAsync(query) ?? await VRChatService.GetUserByDisplaynameAsync(query);
            if (vrcuser != default)
                await VRChatService.SendEventInviteAsync(ctx.Guild, ctx.Member.Id, vrcuser, () => ctx.CreateResponseAsync($"World Invite has ben sent to **{vrcuser.DisplayName}**"));
        }
        
        //[SlashRequireOrbitalManagement]
        [SlashCommand("inv-all", "Invites the member to the world evetnt instance")]
        public async Task InviteAll(InteractionContext ctx)
        {
            await VRChatService.SendEventInviteToAll(async () => await ctx.CreateResponseAsync($"World Invite has ben sent to all members"));
        }

        [SlashRequireRole(RoleCheckMode.Any, "VRCLinked")]
        [SlashCommand("add", "Have our discord bot send you a vrc friend request so that you can get world invites")]
        public async Task VRCAdd(InteractionContext ctx)
        {
            await VRChatService.SendFriendRequestAsync(ctx.Member);
        }
        
        //[SlashRequireOrbitalManagement]
        [SlashCommand("add-them", "Have our discord bot send you a vrc friend request so that you can get world invites")]
        public async Task VRCAddThem(InteractionContext ctx, [Option("Member", "The member who the bot needs to add")] DiscordUser user)
        {
            var role = ctx.Guild.Roles.FirstOrDefault(x => x.Value.Name == "VRCLinked").Value;
            if (!((DiscordMember)user).Roles.Contains(role))
            {
                var wb = new DiscordWebhookBuilder();
                var emb3 = new DiscordEmbedBuilder();
                emb3.WithDescription($"Sorry {((DiscordMember)user).DisplayName} has not yet registered their VRChat name with me yet.");
                wb.AddEmbed(emb3);
                await ctx.EditResponseAsync(wb);
                return;
            }

            var vrcuser = await VRChatService.SendFriendRequestAsync((DiscordMember)user);
            var dwb = new DiscordWebhookBuilder();
            var em = new DiscordEmbedBuilder();
            em.WithDescription($"Friend request has ben sent to **{vrcuser.DisplayName}**");
            dwb.AddEmbed(em);
            await ctx.EditResponseAsync(dwb);
        }
        
        //[SlashRequireOrbitalManagement]
        [SlashCommand("un-add-them", "Have our discord bot unfriend the user")]
        public async Task VRCUnFriendThem(InteractionContext ctx, [Option("Member", "The member who the bot needs to add")] DiscordUser user)
        {
            var role = ctx.Guild.Roles.FirstOrDefault(x => x.Value.Name == "VRCLinked").Value;
            if (!((DiscordMember)user).Roles.Contains(role))
            {
                var wb = new DiscordWebhookBuilder();
                var emb3 = new DiscordEmbedBuilder();
                emb3.WithDescription($"Sorry {((DiscordMember)user).DisplayName} has not yet registered their VRChat name with me yet.");
                wb.AddEmbed(emb3);
                await ctx.EditResponseAsync(wb);
                return;
            }

            var vrcuser = await VRChatService.SendUnFriendRequestAsync((DiscordMember)user);
            var dwb = new DiscordWebhookBuilder();
            var em = new DiscordEmbedBuilder();
            em.WithDescription($"No longer friends with **{vrcuser.DisplayName}**");
            dwb.AddEmbed(em);
            await ctx.EditResponseAsync(dwb);
        }
    }
}