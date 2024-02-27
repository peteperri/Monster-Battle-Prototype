/*a stat is a trait of a Monster that is used when fighting to calculate various values, like damage dealt/taken
 
  Health = HP from Pokemon
 Strength = Attack from Pokemon (AKA Physical Attack)
 Defense = Defense from Pokemon (unchanged, AKA Physical Defense)
 Intelligence = Special Attack from Pokemon 
 Resilience = Special Defense from Pokemon
 Readiness = Speed from Pokemon 
 Reflex = New stat for new battle system; not yet implemented as we are still just replicating Pokemon.
 
 */
public enum Stat
{
    Health, Strength, Defense, Intelligence, Resilience, Readiness, Reflex
}
