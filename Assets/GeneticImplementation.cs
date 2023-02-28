using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using DotNetGraph;
using DotNetGraph.Node;
using DotNetGraph.Edge;
using DotNetGraph.Extensions;
using UnityEngine.Assertions;
using Random = UnityEngine.Random;
using System.IO;
using System.Runtime.InteropServices;
using UnityEngine.Serialization;
using Debug = UnityEngine.Debug;

public enum actions
{
    Add,
    Substract,
    multiply,
    sin,
    cos,
    pow2,
    value,
    
}

public struct NodeT
{
    public actions Acion;
    public NodeT[] Childs;
    public int countchild;
    public DotNode dipslayNode;


    public NodeT(actions act,int countchild,DotNode dipslayNode)
    {
        this.Childs = new NodeT[countchild];
        this.Acion = act;
        this.countchild = countchild;
        this.dipslayNode = dipslayNode;
    }

    public NodeT(NodeT node)
    {
        this.Childs = new NodeT[node.Childs.Length];
        node.Childs.CopyTo(this.Childs,0);
        this.Acion = node.Acion;
        this.countchild = node.countchild;
        this.dipslayNode = node.dipslayNode;
    }
    
    public NodeT(string random)
    {
        Array values = Enum.GetValues(typeof(actions));
        this.Acion = (actions)values.GetValue(Random.Range(0,values.Length));
        
        switch (this.Acion)
        {
            case actions.cos:
                this.countchild = 1;
                break;
            case actions.sin:
                this.countchild = 1;
                break;
            case actions.pow2:
                this.countchild = 1;
                break;
            case actions.value:
                this.countchild = 0;
                break;
            default:
                this.countchild = 2;
                break;
        }

        this.dipslayNode = null;
        this.Childs = new NodeT[this.countchild];
    }

    
    
    public void Swap(NodeT t)
    {
        this.Acion = t.Acion;
        this.Childs = new NodeT[t.Childs.Length];
        t.Childs.CopyTo(this.Childs,0);
        this.countchild = this.Childs.Length;
        this.dipslayNode = t.dipslayNode;
    }
    
}

public class Person
{
    public double score;
    public NodeT Three;

}

public class GeneticImplementation : MonoBehaviour
{
    
    
    public Dictionary<actions, string> labelValue;
    public int nbIndivudus;
    public List<double> lstX;
    public Person[] population;
    public MeshRenderer MR;
    public float minRange;
    public float maxRange;
    public int nbPoint = 20;
    
    private void Start()
    {
        Random.InitState(System.DateTime.Now.Millisecond);
        
        labelValue = new Dictionary<actions, string>(); 
        labelValue.Add(actions.Add,"+");
        labelValue.Add(actions.multiply,"*");
        labelValue.Add(actions.Substract,"-");
        labelValue.Add(actions.value,"x");
        labelValue.Add(actions.cos,"cos");
        labelValue.Add(actions.pow2,"pow2");
        labelValue.Add(actions.sin,"sin");

        lstX = new List<double>();
        population = new Person[nbIndivudus];
        
        for (int i = 0; i < nbPoint; i++)
        {
            lstX.Add(Random.Range(minRange,maxRange));
        }

        for (int i = 0; i < nbIndivudus; i++)
        {

            Person person = new Person();
            person.Three = GenerateThree();
            population[i] = person;
        }

        StartCoroutine(processGenetic());
    }
    
    
    IEnumerator processGenetic()
    {
        int it = 0;
        Person best = new Person();
        best.score = Double.MaxValue;
        while (it < 50)
        {
            it++;
            for (int i = 0; i < population.Length; i++)
            {
                population[i] = fitness(population[i],basicfunction);
            }
            
            population = Sort(population);

            List<Person> bufferList = new List<Person>();
            for (int i = 0; i < population.Length; i++)
            {
                int v = (int)(Mathf.InverseLerp(0, population.Length - 1, i) * 100)+1;
                for (int j = 0; j < v; j++)
                {
                    bufferList.Add(population[i]);
                }
            }

            if (best.score > population[^1].score  )
            {
                best.Three = new NodeT(actions.value,0,null);
                best.Three = CopyNode( (population[^1].Three));
                best.score = population[^1].score;
                
                Debug.Log("gen :"+it+ " "+best.score+" "+ ShowEquation(best.Three));
                DisplayThree(best.Three,"Result",(long)best.score);
                
                if(best.score == 0) break;
            }

            Debug.Log("gen :"+it+ " "+best.score+" ");
            
                
            for (int i = 0; i < nbIndivudus; i++)
            {
                int a = Random.Range(0, bufferList.Count);
                int b = Random.Range(0, bufferList.Count);
                int idx = 0;
                while (bufferList[a] == bufferList[b] && idx < 10000)
                {
                    b = Random.Range(0, bufferList.Count);
                    idx++;
                }

                Person pers = new Person();
                pers.Three = CrosseOver( bufferList[a].Three,  bufferList[b].Three);

                population[i] = pers;
            }

            int k = Random.Range(0, population.Length);
            population[k].Three = (best.Three);

            yield return new WaitForEndOfFrame();
        }
        
        
       // DisplayThree(best.Three,"Result",(long)best.score);
       Debug.Log("gen :"+it+ " "+best.score+" "+ ShowEquation(best.Three));
    }
    
    public double basicfunction(double value)
    {
        //return Math.Pow(value, 4) + value * Math.Cos(value * 10) - Math.Sin(-value) + value;
        //return Math.Cos(Math.Sin(value)) * value * Math.Sin(value);
        return Math.Pow(value, 3) - value + Math.Cos(value * 10);
        //return value + Math.Pow(value, 2) + Math.Pow(value, 3) + Math.Pow(value, 4);
    }   
    
    public double hardFunction(double value)
    {
        return (Math.Exp(value)/value)*.5f;
    }
    
    public Person fitness( Person person,Func<double,double> value)
    {
        double sum = 0;
        for (int i = 0; i < lstX.Count; i++)
        {
            double v = GetValue(person.Three, lstX[i]);

            double fitnessfunction = value.Invoke(lstX[i]);
            sum +=  Math.Abs( v - fitnessfunction);
        }

        person.score = sum;
        return person;
    }
    
    // Start is called before the first frame update
    NodeT GenerateThree()
    {
        NodeT root = new NodeT(actions.value,0,null);

        do
        {
            root = GenerateRandomNode();
        } while (root.Acion == actions.value);
        
        return GenerateIndividu(root,3);;
    }

    public Person[] Sort(Person[] pop)
    {
        bool change = true;

        while (change)
        {
            change = false;
            for (int i = 0; i < pop.Length-1; i++)
            {
                if (pop[i].score < pop[i + 1].score)
                {
                    change = true;
                    (pop[i], pop[i + 1]) = (pop[i + 1], pop[i]);
                }
            }
        }

        return pop;
    }
    
    public NodeT Mutation(NodeT a)
    {
        int v = Random.Range(1, 5);
        NodeT mutated = new NodeT(actions.value,0,null);
     
        switch (v)
        {
            case 0:
                mutated = CrosseOver( a,  a,false);
                break;
            case 1:
                mutated = PonctuelleMutation( a);
                break;
            case 2:
                mutated = extension( a);
                break;
            case 3:
                mutated = subThree( a);
                break;
            case 4:
                mutated = Conpaction( a);
                break;
        }

        return mutated;
    }

    public NodeT subThree( NodeT a)
    {
        NodeT result = CopyNode(a);
        NodeT select = CopyNode(a);
        int c = 1;
        GetRandomNode(a, ref select, 1,ref c);
        NodeT Random = GenerateThree();

        int r = 1;
        result = changeAt(c, result, Random, ref r);
        
        return result;
    }
    
    public NodeT extension( NodeT a)
    {
        NodeT result = CopyNode(a);
        NodeT select = CopyNode(a);
        int d = 1;
        GetRandomNode(a, ref select, 1,ref d);

        NodeT Random = new NodeT(actions.value,0,null);
        do
        { 
            Random = GenerateRandomNode();
        } while (Random.Acion == actions.value);


        NodeT tmp2 = new NodeT(actions.value,0,null);
        tmp2 = CopyNode(select);
        
        Random.Childs[0] = tmp2;
        for (int i = 1; i < Random.Childs.Length; i++)
        {
            NodeT n = new NodeT(actions.value,0,null);
              
            Random.Childs[i] = n;
        }
        

        Random.countchild = Random.Childs.Length;
        select.countchild = select.Childs.Length;
        int c = 1;
        result = changeAt(d, result, Random, ref c);

        return result;
    }
    public NodeT Conpaction( NodeT a)
    {
        NodeT result = CopyNode(a);
        NodeT select = CopyNode(a);

        int d = 1;
        int idx = 0;
        do
        {
            idx++;
            GetRandomNode(a, ref select, 1,ref d);
        } while (select.Acion == actions.value && idx < 100);

        NodeT Random = new NodeT(actions.value, 0, null);
        int r = 1;
        result = changeAt(d, result, Random, ref r);

        return result;
    }

    public NodeT PonctuelleMutation( NodeT a)
    {
        NodeT result = CopyNode(a);
        NodeT select = result;
        
        int d1 = 1;
        GetRandomNode(result, ref select, 1,ref d1);

        NodeT Random;

        do
        { 
            Random = GenerateRandomNode();
        } while (Random.Acion == actions.value || Random.Acion == select.Acion);
        
        if (Random.countchild == 1 )
        {
            if (select.Childs.Length > 0)
            {
                Random.Childs[0] =  select.Childs[0];    
            }
            else
            {
                NodeT node = new NodeT(actions.value, 0, null);
                Random.Childs[0] = node;
            }
           
        }
        else if (Random.countchild > 1)
        {
            
            if (select.countchild == 0)
            {

                for (int i = 0; i < Random.countchild; i++)
                {
                    NodeT node = new NodeT(actions.value, 0, null);
                    Random.Childs[i] = node;
                }
            }
            else if(select.countchild == 1)
            {
                Random.Childs[0] = select.Childs[0];
                NodeT node = new NodeT(actions.value, 0, null);
                Random.Childs[1] = node;
            }
            else
            {
                Random.Childs = select.Childs;
            }
            
        }

        Random.countchild = Random.Childs.Length;
        int c = 1;
        result = changeAt(d1, result, Random, ref c);

        return result;
    }
    
    public void generaList(ref NodeT Root,ref List<DotNode> lst,ref int k)
    {
        if (k <= 0) return;
        k--;
        Root.dipslayNode = new DotNode(Root.GetHashCode().ToString()) {Label = labelValue[Root.Acion]};
        lst.Add(Root.dipslayNode);
        
        for (int i = 0; i < Root.countchild; i++)
        {
            generaList(ref Root.Childs[i],ref lst,ref k);
        }

    }

    public void generateEdge(ref NodeT Root,ref List<DotEdge> lstEdge,ref int k)
    {

        if (Root.countchild <= 0 || k <= 0) return;
        k--;
        for (int i = 0; i < Root.countchild; i++)
        {
            generateEdge(ref Root.Childs[i],ref lstEdge,ref k);
            var myEdge = new DotEdge(Root.dipslayNode,Root.Childs[i].dipslayNode);
            lstEdge.Add(myEdge);
        }

    }
    public void DisplayThree(NodeT Root,string name,long score = 0)
    {
        var graph = new DotGraph("MyGraph");


        List<DotNode> lst = new List<DotNode>();
        List<DotEdge> lstEdge = new List<DotEdge>();
        int k = 5000;
        generaList(ref Root,ref lst,ref k);
        k = 5000;
        generateEdge(ref Root,ref lstEdge,ref k);

        for (int i = 0; i < lst.Count; i++)
        {
            graph.Elements.Add(lst[i]);
        }
        
        for (int i = 0; i < lstEdge.Count; i++)
        {
            graph.Elements.Add(lstEdge[i]);
        }
        
        var dot = graph.Compile();

        File.WriteAllText(Application.dataPath+"/Resources/Graphviz/Data/file"+name+".dot",dot);
        string str = ShowEquation(Root).ToString();
       
        System.Diagnostics.Process process = new System.Diagnostics.Process();
        System.Diagnostics.ProcessStartInfo startInfo = new System.Diagnostics.ProcessStartInfo();
        startInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
        startInfo.FileName = "cmd.exe";
        startInfo.Arguments = "/C dot.exe -Tpng Assets/Resources/Graphviz/Data/file"+name+".dot -o Assets/Resources/Graphviz/"+name+".png -Glabel=\""+str+" score ="+score.ToString() +" \"";
        process.StartInfo = startInfo;
        process.Start();
        
        
        GUIUtility.systemCopyBuffer = str;

        Texture2D image = Resources.Load<Texture2D>("Graphviz/"+name);

        MR.material.mainTexture = image;

    }


    public void isInifite(NodeT root,ref int maxStep)
    {
        if (maxStep < 0) return;
        maxStep--;

        for (int i = 0; i < root.Childs.Length; i++)
        {
            isInifite(root.Childs[i],ref maxStep);
        }
    }

    public struct MyStruct
    {
        public int a;
        public MyStruct[] Childs;
        public MyStruct(int a,int max)
        {
            this.a = a;
            this.Childs = new MyStruct[max];

            for (int i = 0; i < max; i++)
            {
                this.Childs[i] = ( new MyStruct(a,0));
            }
        }

        public MyStruct(MyStruct my)
        {
            this.a = my.a;
            this.Childs = my.Childs;
        }

        public void setA(int a)
        {
            this.a = a;
        }
        
    }

    public MyStruct t(in MyStruct a)
    {
        MyStruct my = a;
        MyStruct tmp = new MyStruct(10, 0);
        my.Childs = new MyStruct[a.Childs.Length];
        for (int i = 0; i < a.Childs.Length; i++)
        {
            my.Childs[i] = new MyStruct( a.Childs[i]);
        }
        my.Childs[0] = (tmp);
        return my;
    }

    public NodeT CopyNode(NodeT a)
    {
        NodeT result = a;
        result.Childs = new NodeT[result.countchild];
        for (int i = 0; i < result.Childs.Length; i++)
        {
            result.Childs[i] = CopyNode(new NodeT(a.Childs[i]));
        }

        return result;
    }
    
    public NodeT CrosseOver(in NodeT a, in NodeT b,bool t=true)
    {
        NodeT result = CopyNode(a);
        NodeT selectA = CopyNode(a);
        NodeT selectB = CopyNode(b);
        
        int idx = 0;
        int d1 = 1;
        int d2 = 1;
        do
        {
            idx++;
            GetRandomNode(a, ref selectA, 1,ref d1);
            GetRandomNode(b, ref selectB, 1,ref d2);
        } while ( ( selectA.Acion == actions.value || selectB.Acion == actions.value) && idx < 20);

        int c = 1;
        result = changeAt(d1, result, selectB, ref c);
        
        if (t && Random.Range(0f, 1f) < .1f )
        { 
            result = Mutation(result);
        }

        
        return result;
    }

    public NodeT changeAt(int count,NodeT root ,NodeT a, ref int buffer)
    {
        if (buffer > count) return root;
        
        if (buffer == count)
        {
            root = a;
            return root;
        }

        for (int i = 0; i < root.Childs.Length; i++)
        {
            buffer++;
            root.Childs[i] = changeAt(count, root.Childs[i], a,ref buffer);
        }

        return root;
    }
    
    public int GetRandomNode(NodeT a,ref NodeT selected,int count, ref int result)
    {

        if (Random.Range(0, count) == count-1)
        {
            selected = a;
            result = count;
        }

        for (int i = 0; i < a.Childs.Length; i++)
        {
            count++;
          
            count = GetRandomNode(a.Childs[i], ref selected, count,ref result);
        }
        
        return count;
    }
    
    public double GetValue(NodeT root,double terminal)
    {
        
        Assert.AreEqual(root.countchild,root.Childs.Length);
        
        double value = 0f;
        double a = 0f;
        double b = 0f;
        
        
        if (root.Childs.Length == 1)
        {
            a = GetValue(root.Childs[0],terminal);    
        }
        else if (root.Childs.Length > 1)
        {
            a = GetValue(root.Childs[0],terminal);    
            b = GetValue(root.Childs[1],terminal);
        }
        
        switch (root.Acion)
        {
            case actions.cos: 
                value= Math.Cos(a);
                break;
            case actions.sin:
                value= Math.Sin(a);
                break;
            case actions.pow2:
                value = Math.Pow(a,2);
                break;
            case actions.multiply:
                value= a*b;
                break;
            case actions.Add:
                value = a + b;
                break;
            case actions.Substract:
                value = a - b;
                break;
            case actions.value:
                value= terminal;
                break;
        }
        
        return value;
    }
    
    public string ShowEquation(NodeT root)
    {
        string Output = "";

        if (root.countchild == 1 && !(root.Acion == actions.pow2))
        {
            Output = (labelValue[root.Acion] + "(" + ShowEquation(root.Childs[0])+")");    
        }
        else if (root.countchild == 1)
        {
            switch (root.Acion)
            {
                case actions.pow2:
                    Output = "(" + ShowEquation(root.Childs[0]) + "^2)";
                    break;
            }
        }
        else if(root.countchild == 2 )
        {
            Output = ( "(" +ShowEquation(root.Childs[0])+" "+ labelValue[root.Acion]+" "+ShowEquation(root.Childs[1])+")");
        }
        else
        {
            Output = labelValue[root.Acion];
        }
        
        return Output;
    }
    
    public NodeT GenerateIndividu(NodeT n,int maxHeight)
    {
        
        if (maxHeight > 0 && n.countchild > 0)
        {
            int h = maxHeight / n.countchild;
            for (int i = 0; i < n.countchild; i++)
            {
                n.Childs[i] = GenerateIndividu(GenerateRandomNode(),h);
            }
        }
        else
        {
            if (n.Acion != actions.value)
            {
                for (int i = 0; i < n.countchild; i++)
                {
                    NodeT node = new NodeT(actions.value,0,null);
                    n.Childs[i] = node;
                }
                
            }
        }

        n.countchild = n.Childs.Length;
        
        return n;
    }

    public NodeT GenerateRandomNode()
    {
        return new NodeT("");
    }
    
}
