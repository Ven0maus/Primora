using Primora.Core.Items.Interfaces;
using Primora.Core.Items.Objects;
using Primora.Core.Npcs;

namespace Primora.Core.Items.Types
{
    internal class Consumable : Item, IConsumable
    {
        public Consumable(ItemConfiguration configuration) : base(configuration)
        {
        }

        public virtual void Consume(Actor actor)
        {
            // TODO
        }
    }
}
