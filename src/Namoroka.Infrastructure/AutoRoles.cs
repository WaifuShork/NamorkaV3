using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace Namoroka.Infrastructure
{
    public class AutoRoles
    {
        private readonly NamorokaContext _context;

        public AutoRoles(NamorokaContext context)
        {
            _context = context;
        }

        public async Task<List<AutoRole>> GetAutoRolesAsync(ulong id)
        {
            var autoRoles = await _context.AutoRoles
                .Where(x => x.ServerId == id)
                .ToListAsync();

            return await Task.FromResult(autoRoles);
        }

        public async Task AddAutoRoleAsync(ulong id, ulong roleId)
        {
            var server = await _context.Servers.FindAsync(id);

            if (server == null)
            {
                _context.Add(new Server {Id = id});
            }

            _context.Add(new AutoRole {RoleId = roleId, ServerId = id});
            await _context.SaveChangesAsync();
        }

        public async Task RemoveAutoRoleAsync(ulong id, ulong roleId)
        {
            var autoRole = await _context.AutoRoles.Where(x => x.RoleId == roleId).FirstOrDefaultAsync();

            _context.Remove(autoRole);
            await _context.SaveChangesAsync();
        }

        public async Task ClearAutoRoleAsync(List<AutoRole> autoRole)
        {
            _context.RemoveRange(autoRole);
            await _context.SaveChangesAsync();
        }
    }
}