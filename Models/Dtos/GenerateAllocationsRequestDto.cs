public class GenerateAllocationsRequestDto
{
    public int Year { get; set; }
    public int Month { get; set; }
    public int ActorUserId { get; set; } // The frontend sends this, good to include
}