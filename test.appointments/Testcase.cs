namespace Test
{
    public class TestCase<TSeed, TInput, TExpect>
    {
        public TSeed[] Given { get; set; }
        public TInput Arguments { get; set; }
        public TExpect Expect { get; set; }
    }
}