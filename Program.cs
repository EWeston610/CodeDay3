namespace day3InClass;

class Course
{
    public Dictionary<int, int> course_availseats = new();
    public Dictionary<int, object> course_locks = new();
    //public readonly object seatLock = new();   // guards course_availseats across threads

    public Course(int courses = 100, int availseats = 10_000)
    {
        for (int i = 1; i <= courses; i++)
        {
            course_availseats[i] = availseats;
            course_locks[i] = new object();
        }
    }
}

class Student
{
    public int id;
    public List<int> courses;

    public Student(int in_id)
    {
        id = in_id;
        courses = new();
    }

    public void SelectCoursesForRegistration(int num)
    {
        while (courses.Count < num)
        {
            int pick = Random.Shared.Next(100) + 1; // 1-100
            if (!courses.Contains(pick))            // don't pick the same course twice
                courses.Add(pick);
        }
    }

    public bool RegisterMyCourses()
    {
        bool allRegistered = true;

        int firstID = courses[0];
        int secondID = courses[1];

        if(firstID > secondID)
        {
            int temp = firstID;
            firstID = secondID;
            secondID = temp;
        }

        lock (Program.c.course_locks[firstID])
        {
            //Thread.Sleep(1); //delay to force overlap
            lock (Program.c.course_locks[secondID])
            {
                foreach (var c in courses)
                {
                    if (Program.c.course_availseats[c] > 0)
                        Program.c.course_availseats[c]--;
                    else
                        allRegistered = false;          // course is full
                }
            }
        }
        return allRegistered;
    }
}

class Program
{
    public static Course c = new();

    static void Main(string[] args)
    {
        List<Thread> myThreads = new();
        for (int i = 1; i < 200; i++)
        {
            // 1) Create a thread to do N (1000) student registrations
            myThreads.Add(new Thread(() =>
            {
                for (int i = 1; i <= 1000; i++)
                {
                    Student s = new Student(i);          // create student
                    s.SelectCoursesForRegistration(2);   // Student.SelectCoursesForRegistration
                    if (s.RegisterMyCourses() == false)               // Student.RegisterMyCourses
                        Console.WriteLine($"Student {s.id} failed to register course list");
                }
            }));
        }

        foreach (var x in myThreads)
        {
            x.Start();
        }

        foreach (var x in myThreads)
        {
            x.Join();
        }
        Console.WriteLine("All seats assigned");
        foreach (var x in c.course_availseats)
        {
            Console.WriteLine($"Course {x.Key} has {x.Value} seats still available");
        }
    }
}