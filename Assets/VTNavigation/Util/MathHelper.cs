namespace VTNavigation.Util
{
    public static class MathHelper
    {
        private static int[] m_Steps;

        private static void InitSteps(int n)
        {
            m_Steps = new int[n+1];
            m_Steps[0] = 1;
            for (int i = 1; i <=n; i++)
            {
                m_Steps[i] = m_Steps[i - 1] * i;
            }
        }

        public static int Step(int n)
        {
            if (m_Steps == null || m_Steps.Length <= n)
            {
                InitSteps(n);
            }
            return m_Steps[n];
        }
    }
}