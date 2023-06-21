using System.Collections.Generic;

public abstract class MaxSizeContainer<T> where T : class
{
    #region State And Properties

    protected abstract uint MaxSize { get; }
    private List<T> Elements { get; set; }
    public int CurrentSize
    {
        get
        {
            return Elements.Count;
        }
    }

    #endregion

    #region Constructors And Initialization

    public MaxSizeContainer()
    {
        Elements = new List<T>();
    }

    #endregion

    #region Methods

    public List<T> GetElements()
    {
        List<T> returnedElements = new List<T>();

        foreach (T returnedElement in Elements)
        {
            returnedElements.Add(returnedElement);
        }

        return returnedElements;
    }

    public T GetElement(int index)
    {
        T element = null;

        bool invalidIndex = index < 1 || index > CurrentSize;

        if (!invalidIndex)
        {
            element = Elements[index - 1];
        }

        return element;
    }

    public bool AddElement(T elementToAdd)
    {
        if (Elements.Contains(elementToAdd))
        {
            return false;
        }
        
        bool added = false;

        if (CurrentSize < MaxSize)
        {
            Elements.Add(elementToAdd);
            added = true;
        }

        return added;
    }

    public T RemoveElement(int indexOfRemoved)
    {
        T removedElement = null;

        bool invalidIndex = indexOfRemoved < 1 || indexOfRemoved > CurrentSize;

        if (!invalidIndex)
        {
            removedElement = Elements[indexOfRemoved - 1];
            Elements.RemoveAt(indexOfRemoved - 1);
        }

        return removedElement;
    }

    public T ReplaceElement(T elementToAdd, int indexOfReplaced)
    {
        if (Elements.Contains(elementToAdd))
        {
            return null;
        }

        T replacedElement = null;

        bool invalidIndex = indexOfReplaced < 1 || indexOfReplaced > CurrentSize;

        if (!invalidIndex)
        {
            replacedElement = Elements[indexOfReplaced - 1];
            Elements[indexOfReplaced - 1] = elementToAdd;
        }

        return replacedElement;
    }

    public bool SwapElements(int index1, int index2)
    {
        bool swapped = false;

        bool invalidIndexes = (index1 < 1 || index1 > CurrentSize) ||
                              (index2 < 1 || index2 > CurrentSize) ||
                              (index1.Equals(index2));

        if (!invalidIndexes)
        {
            T temp = Elements[index1 - 1];
            Elements[index1 - 1] = Elements[index2 - 1];
            Elements[index2 - 1] = temp;

            swapped = true;
        }

        return swapped;
    }

    #endregion
}
