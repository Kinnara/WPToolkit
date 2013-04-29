// (c) Copyright Microsoft Corporation.
// This source is subject to the Microsoft Public License (Ms-PL).
// Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
// All other rights reserved.

using System.Collections.Generic;

namespace PhoneToolkitSample.Data
{
    public class PeopleByFirstName : List<PeopleInGroup>
    {
        private const string Groups = "#abcdefghijklmnopqrstuvwxyz";

        private const string GlobeGroupKey = "\uD83C\uDF10";

        private Dictionary<int, Person> _personLookup = new Dictionary<int, Person>();

        public PeopleByFirstName()
        {
            List<Person> people = new List<Person>(AllPeople.Current);
            people.Sort(Person.CompareByFirstName);

            Dictionary<string, PeopleInGroup> groups = new Dictionary<string, PeopleInGroup>();

            foreach (char c in Groups)
            {
                PeopleInGroup group = new PeopleInGroup(c.ToString());
                this.Add(group);
                groups[c.ToString()] = group;
            }

            //Globe group
            PeopleInGroup globegroup = new PeopleInGroup(GlobeGroupKey);
            this.Add(globegroup);
            groups[GlobeGroupKey] = globegroup;

            foreach (Person person in people)
            {
                groups[Person.GetFirstNameKey(person)].Add(person);
            }
        }
    }
}
